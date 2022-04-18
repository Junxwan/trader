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
        private readonly string sourceDir;

        private Dictionary<string, Dictionary<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>> data;

        public Transaction(string sourceDir)
        {
            this.sourceDir = sourceDir + "\\op";
            this.data = new Dictionary<string, Dictionary<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>>();
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
                            if (item.DateTime.Subtract(vv.DateTime).TotalSeconds >= 300 || index == cp.Value.Count - 1)
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

                        CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture);

                        foreach (KeyValuePair<string, List<Csv.MinPrice>> cdata in minData)
                        {
                            string f = pdir + "\\" + cdata.Key + "-" + cp.Key;

                            if (fileDate.ToString("yyyy-MM-dd") != cdata.Key)
                            {
                                f += "-night";
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

        //5分K資料
        public SortedList<string, Dictionary<string, List<Csv.MinPrice>>> Get5MinK(string period, DateTime dateTime)
        {
            if (!this.data.ContainsKey(period))
            {
                this.data[period] = new Dictionary<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>();
            }
            else if (this.data[period].ContainsKey(dateTime))
            {
                return this.data[period][dateTime];
            }

            var dir = this.sourceDir + "\\price\\5min\\" + period;
            var files = new string[] { };

            foreach (var info in (new DirectoryInfo(dir)).GetDirectories())
            {
                var date = dateTime.ToString("yyyy-MM-dd");
                var fn = "";
                foreach (var file in info.GetFiles("*.csv").OrderBy(f => f.Name))
                {
                    if (file.Name.StartsWith(date))
                    {
                        var yd = fn == "" ? "" : fn.Substring(0, 10);

                        files = new string[] {
                        file.DirectoryName+"\\"+yd+"-P.csv",
                        file.DirectoryName+"\\"+yd+"-P-night.csv",
                        file.DirectoryName+"\\"+yd+"-C.csv",
                        file.DirectoryName+"\\"+yd+"-C-night.csv",

                        file.DirectoryName+"\\"+date+"-P.csv",
                        file.DirectoryName+"\\"+date+"-P-night.csv",
                        file.DirectoryName+"\\"+date+"-C.csv",
                        file.DirectoryName+"\\"+date+"-C-night.csv"
                        };
                    }
                    else
                    {
                        fn = file.Name;
                    }
                }

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

                    var k = DateTime.Parse(name.Substring(0, 10));
                    if (!this.data[period].ContainsKey(k))
                    {
                        this.data[period][k] = new SortedList<string, Dictionary<string, List<Csv.MinPrice>>>();
                    }

                    if (!this.data[period][k].ContainsKey(info.Name))
                    {
                        var op = new Dictionary<string, List<Csv.MinPrice>>();
                        op["call"] = new List<Csv.MinPrice>();
                        op["put"] = new List<Csv.MinPrice>();

                        this.data[period][k][info.Name] = op;
                    }

                    using var reader = new StreamReader(file);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    {
                        foreach (var item in csv.GetRecords<Csv.MinPrice>())
                        {
                            switch (name[11])
                            {
                                case 'P':
                                    this.data[period][k][info.Name]["put"].Add(item);
                                    break;
                                case 'C':
                                    this.data[period][k][info.Name]["call"].Add(item);
                                    break;
                            }
                        }
                    }
                }
            }

            return this.data[period][dateTime];
        }

        public SortedList<string, Dictionary<string, List<Csv.MinPrice>>> PrevGet5MinK(string period, DateTime dateTime)
        {
            var dates = this.data[period].Keys.ToArray();
            var index = Array.IndexOf(dates, dateTime);
            if (index <= 0)
            {
                var dir = this.sourceDir + "\\price\\5min\\" + period;
                var yn = "";
                foreach (var file in (new DirectoryInfo(dir)).GetFiles("*.csv"))
                {
                    if (file.Name.Substring(0, 10) == dateTime.ToString("yyyy-MM-dd"))
                    {
                        if (yn != "")
                        {
                            dateTime = DateTime.Parse(yn.Substring(0, 10));

                        }

                        break;
                    }
                    else
                    {
                        yn = file.Name;
                    }
                }

                this.Get5MinK(period, dateTime);
                dates = this.data[period].Keys.ToArray();
                index = Array.IndexOf(dates, dateTime);
            }

            return this.data[period][dates[index - 1]];
        }

        public SortedList<string, Dictionary<string, List<Csv.MinPrice>>> NextGet5MinK(string period, DateTime dateTime)
        {
            var dates = this.data[period].Keys.ToArray();
            var index = Array.IndexOf(dates, dateTime);
            if (dates.Length < (index + 2))
            {
                var dir = this.sourceDir + "\\price\\5min\\" + period;
                var yn = "";
                foreach (var file in (new DirectoryInfo(dir)).GetFiles("*.csv"))
                {
                    if (file.Name.Substring(0, 10) == dateTime.ToString("yyyy-MM-dd"))
                    {
                        if (yn != "")
                        {
                            dateTime = DateTime.Parse(yn.Substring(0, 10));

                        }

                        break;
                    }
                    else
                    {
                        yn = file.Name;
                    }
                }

                this.Get5MinK(period, dateTime);
                dates = this.data[period].Keys.ToArray();
                index = Array.IndexOf(dates, dateTime);
            }

            return this.data[period][dates[index + 1]];
        }

        public Dictionary<string, Dictionary<string, Csv.MinPrice>> PrevGetLast5MinK(string period, DateTime dateTime, bool IsDayPlate = true)
        {
            var colsePrice = new Dictionary<string, Dictionary<string, Csv.MinPrice>>();
            SortedList<string, Dictionary<string, List<Csv.MinPrice>>> data;
            DateTime startDateTime;
            DateTime endDateTime;

            if (IsDayPlate)
            {
                data = this.PrevGet5MinK(period, dateTime);
                var dates = this.data[period].Keys.ToArray();
                var index = Array.IndexOf(dates, dateTime);
                startDateTime = new DateTime(dates[index - 1].Year, dates[index - 1].Month, dates[index - 1].Day, 8, 45, 0);
            }
            else
            {
                data = this.Get5MinK(period, dateTime);
                startDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 8, 45, 0);
            }

            endDateTime = startDateTime.AddHours(5);

            foreach (KeyValuePair<string, Dictionary<string, List<Csv.MinPrice>>> row in data)
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
    }
}
