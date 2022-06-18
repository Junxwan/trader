using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.OPS
{
    public class Transaction
    {
        //op資料目錄
        public readonly string sourceDir;

        private readonly string priceDir;

        private Dictionary<string, SortedList<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>> data;

        private Dictionary<string, Dictionary<string, List<string>>> PriceDates;

        private Calendar Calendar;

        public Transaction(string sourceDir)
        {
            this.sourceDir = sourceDir + "\\op";
            this.priceDir = this.sourceDir + "\\price\\5min\\";
            this.data = new Dictionary<string, SortedList<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>>();
            this.PriceDates = new Dictionary<string, Dictionary<string, List<string>>>();
            this.Calendar = new Calendar(this.sourceDir);
        }

        public bool ToMinPriceCsv(DateTime fileDate, string file, string[] periods, int min = 5, string type = "TXO")
        {
            var records = new Dictionary<string, Dictionary<int, Dictionary<string, List<Csv.Transaction>>>>();

            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            {
                csv.Read();
                csv.ReadHeader();
                int index = 0;
                while (csv.Read())
                {
                    index++;
                    if (index == 1 || csv.GetField(1).Trim() != type || !periods.Contains(csv.GetField<string>(3).Trim()))
                    {
                        continue;
                    }

                    string d = csv.GetField<string>(0).Trim().Insert(4, "-").Insert(7, "-");
                    int t = csv.GetField<int>(5);
                    int h = t / 10000;
                    int m = (t - (h * 10000)) / 100;
                    int s = t - (h * 10000 + m * 100);

                    var record = new Csv.Transaction
                    {
                        DateTime = DateTime.Parse(d + " " + h.ToString() + ":" + m.ToString() + ":" + s.ToString()),
                        Type = "TXO",
                        Performance = csv.GetField<int>(2),
                        Period = csv.GetField<string>(3).Trim(),
                        Cp = csv.GetField<string>(4).Trim(),
                        Price = csv.GetField<double>(6),
                        Volume = csv.GetField<int>(7),
                    };

                    if (!records.ContainsKey(record.Period))
                    {
                        records.Add(record.Period, new Dictionary<int, Dictionary<string, List<Csv.Transaction>>>());
                    }

                    if (!records[record.Period].ContainsKey(record.Performance))
                    {
                        var pcp = new Dictionary<string, List<Csv.Transaction>>();
                        pcp.Add("C", new List<Csv.Transaction>());
                        pcp.Add("P", new List<Csv.Transaction>());
                        records[record.Period].Add(record.Performance, pcp);
                    }

                    records[record.Period][record.Performance][record.Cp].Add(record);
                }
            }

            foreach (KeyValuePair<string, Dictionary<int, Dictionary<string, List<Csv.Transaction>>>> row in records)
            {
                var dir = this.sourceDir + "\\price\\" + min.ToString() + "min\\" + row.Key;
                Dictionary<string, List<Csv.MinPrice>> minData;

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                foreach (KeyValuePair<int, Dictionary<string, List<Csv.Transaction>>> prow in row.Value)
                {
                    var pdir = dir + "\\" + prow.Key;

                    if (!Directory.Exists(pdir))
                    {
                        Directory.CreateDirectory(pdir);
                    }

                    foreach (KeyValuePair<string, List<Csv.Transaction>> cp in prow.Value)
                    {
                        if (cp.Value.Count == 0)
                        {
                            continue;
                        }

                        cp.Value.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));

                        minData = new Dictionary<string, List<Csv.MinPrice>>();
                        var vv = new Csv.MinPrice()
                        {
                            Open = cp.Value[0].Price,
                            High = cp.Value[0].Price,
                            Low = cp.Value[0].Price,
                            Close = cp.Value[0].Price,
                            DateTime = cp.Value[0].DateTime.AddSeconds(-cp.Value[0].DateTime.Second)
                        };

                        vv.DateTime = vv.DateTime.AddMinutes(-(cp.Value[0].DateTime.Minute % 5));
                        int index = 0;

                        foreach (var item in cp.Value)
                        {
                            if (item.DateTime.Subtract(vv.DateTime).TotalSeconds >= 300)
                            {
                                vv.Volume = vv.Volume / 2;
                                var date = vv.DateTime.ToString("yyyy-MM-dd");

                                if (!minData.ContainsKey(date))
                                {
                                    minData[date] = new List<Csv.MinPrice>();
                                }

                                minData[date].Add(vv);
                                vv = new Csv.MinPrice()
                                {
                                    Open = item.Price,
                                    High = item.Price,
                                    Low = item.Price,
                                    Close = item.Price,
                                    DateTime = item.DateTime.AddSeconds(-item.DateTime.Second)
                                };

                                vv.DateTime = vv.DateTime.AddMinutes(-(item.DateTime.Minute % 5));
                            }

                            vv.Volume += item.Volume;
                            vv.Close = item.Price;

                            if (item.Price > vv.High)
                            {
                                vv.High = item.Price;
                            }

                            if (item.Price < vv.Low)
                            {
                                vv.Low = item.Price;
                            }

                            index++;
                        }

                        // 最後一筆
                        if (index == cp.Value.Count)
                        {
                            vv.Volume = vv.Volume / 2;
                            var date = vv.DateTime.ToString("yyyy-MM-dd");

                            if (!minData.ContainsKey(date))
                            {
                                minData[date] = new List<Csv.MinPrice>();
                            }

                            minData[date].Add(vv);
                        }

                        CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture);

                        foreach (KeyValuePair<string, List<Csv.MinPrice>> cdata in minData)
                        {
                            string f = pdir + "\\" + cdata.Key + "-" + cp.Key;
                            DateTime startTime = new DateTime();
                            DateTime endTime = new DateTime();
                            int[] removeRange = new int[] { };

                            if (fileDate.ToString("yyyy-MM-dd") != cdata.Key)
                            {
                                f += "-night";

                                // 夜盤資料是1500-0000
                                startTime = DateTime.Parse(cdata.Key + " 15:00:00");
                                endTime = startTime.AddHours(9);

                                // 如果夜盤資料是週六，則只會有0000-0455資料
                                if (startTime.DayOfWeek == DayOfWeek.Saturday)
                                {
                                    startTime = DateTime.Parse(cdata.Key + " 00:00:00");
                                    endTime = startTime.AddHours(5);
                                }
                            }

                            // 開倉日資料只有0845-1345
                            else if (Calendar.GetFirstDate(row.Key) == DateTime.Parse(cdata.Key))
                            {
                                startTime = DateTime.Parse(cdata.Key + " 08:45:00");
                                endTime = startTime.AddHours(5);
                            }
                            else
                            {
                                startTime = DateTime.Parse(cdata.Key + " 00:00:00");
                                endTime = startTime.AddHours(13).AddMinutes(45);

                                // 移除0500-0840
                                removeRange = new int[] {
                                    (int)(startTime.AddHours(5)-startTime).TotalMinutes/5,
                                    (int)(startTime.AddHours(8).AddMinutes(45)-startTime.AddHours(5)).TotalMinutes/5,
                                };
                            }

                            // 如果沒有開盤資料則用最近一筆
                            if (cdata.Value[0].DateTime != startTime)
                            {
                                cdata.Value.Insert(0, new Csv.MinPrice()
                                {
                                    DateTime = startTime,
                                    Open = cdata.Value[0].Close,
                                    Close = cdata.Value[0].Close,
                                    High = cdata.Value[0].Close,
                                    Low = cdata.Value[0].Close,
                                });
                            }

                            // 回補缺失的時間資料
                            for (int i = 0; i < (endTime - startTime).TotalMinutes / 5; i++)
                            {
                                var d = startTime.AddMinutes(i * 5);

                                if (cdata.Value.Count <= i || cdata.Value[i].DateTime != d)
                                {
                                    cdata.Value.Insert(i, new Csv.MinPrice()
                                    {
                                        DateTime = d,
                                        Open = cdata.Value[i - 1].Close,
                                        Close = cdata.Value[i - 1].Close,
                                        High = cdata.Value[i - 1].Close,
                                        Low = cdata.Value[i - 1].Close,
                                    });
                                }
                            }

                            // 移除多回補的資料
                            if (removeRange.Length > 0)
                            {
                                cdata.Value.RemoveRange(removeRange[0], removeRange[1]);

                                if (removeRange.Length == 4)
                                {
                                    cdata.Value.RemoveRange(removeRange[2], removeRange[3]);
                                }
                            }

                            f += ".csv";

                            if (!File.Exists(f))
                            {
                                FileStream fs = File.Create(f);
                                fs.Close();
                                fs.Dispose();
                            }

                            using var writer = new StreamWriter(f, false, Encoding.UTF8);
                            using var csvw = new CsvWriter(writer, csvConfig);
                            csvw.WriteRecords(cdata.Value);
                        }
                    }
                }
            }

            return true;
        }

        public void LoadPriceFiles(string period)
        {
            if (this.PriceDates.ContainsKey(period))
            {
                return;
            }

            this.PriceDates[period] = new Dictionary<string, List<string>>();

            var dir = this.priceDir + period;
            foreach (var info in (new DirectoryInfo(dir)).GetDirectories())
            {
                var hash = new HashSet<string>();

                foreach (var file in info.GetFiles("*.csv"))
                {
                    hash.Add(file.Name.Substring(0, 10));
                }

                this.PriceDates[period][info.Name] = hash.ToList();
                this.PriceDates[period][info.Name].Sort((x, y) => -x.CompareTo(y));
            }
        }

        //5分K資料
        public SortedList<string, Dictionary<string, List<Csv.MinPrice>>> Get5MinK(string period, DateTime dateTime)
        {
            if (!this.data.ContainsKey(period))
            {
                this.data[period] = new SortedList<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>();
            }
            else if (this.data[period].ContainsKey(dateTime))
            {
                return this.data[period][dateTime];
            }

            this.LoadPriceFiles(period);

            foreach (var prices in this.PriceDates[period])
            {
                var date = dateTime.ToString("yyyy-MM-dd");
                var pDir = this.priceDir + "\\" + period + "\\" + prices.Key;

                if (!prices.Value.Contains(dateTime.ToString("yyyy-MM-dd")))
                {
                    continue;
                }

                var files = new string[] {
                        pDir+"\\"+date+"-P.csv",
                        pDir+"\\"+date+"-P-night.csv",
                        pDir+"\\"+date+"-C.csv",
                        pDir+"\\"+date+"-C-night.csv"
                        };

                foreach (var file in files)
                {
                    if (!File.Exists(file))
                    {
                        continue;
                    }

                    var name = Path.GetFileName(file);
                    if ((name[11] != 'P' && name[11] != 'C'))
                    {
                        continue;
                    }

                    var k = DateTime.Parse(date);
                    if (!this.data[period].ContainsKey(k))
                    {
                        this.data[period][k] = new SortedList<string, Dictionary<string, List<Csv.MinPrice>>>();
                    }

                    if (!this.data[period][k].ContainsKey(prices.Key))
                    {
                        var op = new Dictionary<string, List<Csv.MinPrice>>();
                        op["call"] = new List<Csv.MinPrice>();
                        op["put"] = new List<Csv.MinPrice>();

                        this.data[period][k][prices.Key] = op;
                    }

                    using var reader = new StreamReader(file);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    {
                        foreach (var item in csv.GetRecords<Csv.MinPrice>())
                        {
                            switch (name[11])
                            {
                                case 'P':
                                    this.data[period][k][prices.Key]["put"].Add(item);
                                    break;
                                case 'C':
                                    this.data[period][k][prices.Key]["call"].Add(item);
                                    break;
                            }
                        }
                    }
                }
            }

            if (!this.data[period].ContainsKey(dateTime))
            {
                return new SortedList<string, Dictionary<string, List<Csv.MinPrice>>>();
            }

            return this.data[period][dateTime];
        }

        public SortedList<string, Dictionary<string, List<Csv.MinPrice>>> Get5MinKRange(string period, DateTime startDate, DateTime endDate)
        {
            var date = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            var value = new SortedList<string, Dictionary<string, List<Csv.MinPrice>>>();

            for (int i = 0; i <= Math.Round((endDate - startDate).TotalDays); i++)
            {
                foreach (KeyValuePair<string, Dictionary<string, List<Csv.MinPrice>>> row in this.Get5MinK(period, date.AddDays(i)))
                {
                    if (value.ContainsKey(row.Key))
                    {
                        value[row.Key]["call"].AddRange(row.Value["call"]);
                        value[row.Key]["put"].AddRange(row.Value["put"]);
                    }
                    else
                    {
                        value[row.Key] = row.Value;
                    }
                }
            }

            return value;
        }

        public Dictionary<string, Dictionary<string, Csv.MinPrice>> GetLast5MinK(string period, DateTime dateTime)
        {
            var colsePrice = new Dictionary<string, Dictionary<string, Csv.MinPrice>>();
            SortedList<string, Dictionary<string, List<Csv.MinPrice>>> data;
            var startDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 8, 45, 0);
            var endDateTime = startDateTime.AddHours(5);

            foreach (KeyValuePair<string, Dictionary<string, List<Csv.MinPrice>>> row in this.Get5MinK(period, dateTime))
            {
                colsePrice[row.Key] = new Dictionary<string, Csv.MinPrice>();
                colsePrice[row.Key]["call"] = new Csv.MinPrice();
                colsePrice[row.Key]["put"] = new Csv.MinPrice();

                foreach (var cp in new string[] { "call", "put" })
                {
                    foreach (var value in row.Value[cp])
                    {
                        if (value.DateTime >= startDateTime && value.DateTime <= endDateTime)
                        {
                            colsePrice[row.Key][cp].Close = value.Close;
                            colsePrice[row.Key][cp].DateTime = value.DateTime;
                        }
                    }
                }
            }

            return colsePrice;
        }

        public List<string> GetFiles()
        {
            var files = new List<string>();
            var dir = this.sourceDir + "\\transaction";
            if (Directory.Exists(dir))
            {
                foreach (var file in (new DirectoryInfo(this.sourceDir + "\\transaction")).GetFiles("*.zip"))
                {
                    files.Add(file.Name);
                }

                files.Sort((x, y) => -x.CompareTo(y));
            }

            return files;
        }

        public List<string> Get5MinFiles(string period)
        {
            var files = new List<string>();
            var dir = this.priceDir + period;

            if (Directory.Exists(dir))
            {
                foreach (var file in (new DirectoryInfo(dir)).GetFiles("*.csv"))
                {
                    files.Add(file.Name);
                }

                files.Sort((x, y) => -x.CompareTo(y));
            }

            return files;
        }
    }
}
