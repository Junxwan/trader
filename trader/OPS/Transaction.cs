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

        private Dictionary<string, Dictionary<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>> nightData;

        public Transaction(string sourceDir)
        {
            this.sourceDir = sourceDir + "\\op";
            this.data = new Dictionary<string, Dictionary<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>>();
            this.nightData = new Dictionary<string, Dictionary<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>>();
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

                        vv.DateTime = vv.DateTime.AddMinutes(5 - (cp.Value[0].DateTime.Minute % 5));
                        int index = 0;

                        foreach (var item in cp.Value)
                        {
                            if (vv.DateTime <= item.DateTime || index == cp.Value.Count - 1)
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

                                vv.DateTime = vv.DateTime.AddMinutes(5 - (item.DateTime.Minute % 5));
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

        private void Load(string period, DateTime dateTime)
        {
            if (!this.data.ContainsKey(period))
            {
                this.data[period] = new Dictionary<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>();
            }

            if (!this.data[period].ContainsKey(dateTime))
            {
                this.data[period][dateTime] = new SortedList<string, Dictionary<string, List<Csv.MinPrice>>>();
            }

            if (this.data[period][dateTime].Count > 0)
            {
                return;
            }

            var dir = this.sourceDir + "\\price\\5min\\" + period;

            foreach (var info in (new DirectoryInfo(dir)).GetDirectories())
            {
                var op = new Dictionary<string, List<Csv.MinPrice>>();
                var date = dateTime.ToString("yyyy-MM-dd");
                op["call"] = new List<Csv.MinPrice>();
                op["put"] = new List<Csv.MinPrice>();

                foreach (var file in info.GetFiles(date + "*.csv"))
                {
                    if ((file.Name[11] != 'P' && file.Name[11] != 'C') || file.Name.Contains("night"))
                    {
                        continue;
                    }

                    using var reader = new StreamReader(file.FullName);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    {
                        foreach (var item in csv.GetRecords<Csv.MinPrice>())
                        {
                            switch (file.Name[11])
                            {
                                case 'P':
                                    op["put"].Add(item);
                                    break;
                                case 'C':
                                    op["call"].Add(item);
                                    break;
                            }
                        }
                    }
                }

                this.data[period][dateTime].Add(info.Name, op);
            }
        }

        private void LoadNight(string period, DateTime dateTime)
        {
            if (!this.nightData.ContainsKey(period))
            {
                this.nightData[period] = new Dictionary<DateTime, SortedList<string, Dictionary<string, List<Csv.MinPrice>>>>();
            }

            if (!this.nightData[period].ContainsKey(dateTime))
            {
                this.nightData[period][dateTime] = new SortedList<string, Dictionary<string, List<Csv.MinPrice>>>();
            }

            if (this.nightData[period][dateTime].Count > 0)
            {
                return;
            }

            var dir = this.sourceDir + "\\price\\5min\\" + period;

            foreach (var info in (new DirectoryInfo(dir)).GetDirectories())
            {
                var op = new Dictionary<string, List<Csv.MinPrice>>();
                var date = dateTime.ToString("yyyy-MM-dd");
                op["call"] = new List<Csv.MinPrice>();
                op["put"] = new List<Csv.MinPrice>();

                foreach (var file in info.GetFiles(date + "*.csv"))
                {
                    if ((file.Name[11] != 'P' && file.Name[11] != 'C') || !file.Name.Contains("night"))
                    {
                        continue;
                    }

                    using var reader = new StreamReader(file.FullName);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    {
                        foreach (var item in csv.GetRecords<Csv.MinPrice>())
                        {
                            switch (file.Name[11])
                            {
                                case 'P':
                                    op["put"].Add(item);
                                    break;
                                case 'C':
                                    op["call"].Add(item);
                                    break;
                            }
                        }
                    }
                }

                this.nightData[period][dateTime].Add(info.Name, op);
            }
        }

        //5分K資料
        public SortedList<string, Dictionary<string, List<Csv.MinPrice>>> Get5MinK(string period, DateTime dateTime, bool IsDayPlate = true)
        {
            this.Load(period, dateTime);
            if (!IsDayPlate)
            {
                return this.nightData[period][dateTime];
            }
            return this.data[period][dateTime];
        }
    }
}
