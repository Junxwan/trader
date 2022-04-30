using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.Futures
{
    public class Transaction
    {
        private readonly string sourceDir;

        private readonly string priceDir;

        private readonly string costDir;

        private Dictionary<string, SortedList<DateTime, List<MinPriceCsv>>> data;

        private Dictionary<string, SortedList<DateTime, List<CostCsv>>> costData;

        public Dictionary<string, List<string>> PriceDates;

        public Transaction(string sourceDir)
        {
            this.sourceDir = sourceDir + "\\futures";
            this.priceDir = this.sourceDir + "\\price\\5min\\";
            this.costDir = this.sourceDir + "\\cost";
            this.data = new Dictionary<string, SortedList<DateTime, List<MinPriceCsv>>>();
            this.costData = new Dictionary<string, SortedList<DateTime, List<CostCsv>>>();
            this.PriceDates = new Dictionary<string, List<string>>();
        }

        private Dictionary<string, List<TransactionCsv>> GetTransactionCsvs(string file, string? periods = null, string type = "TX")
        {
            var records = new Dictionary<string, List<TransactionCsv>>();
            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    if (csv.GetField(1).Trim() != type)
                    {
                        continue;
                    }

                    if (periods != null && periods != csv.GetField<string>(2).Trim())
                    {
                        continue;
                    }

                    string d = csv.GetField<string>(0).Trim().Insert(4, "-").Insert(7, "-");
                    int t = csv.GetField<int>(3);
                    int h = t / 10000;
                    int m = (t - (h * 10000)) / 100;
                    int s = t - (h * 10000 + m * 100);

                    if (!records.ContainsKey(d))
                    {
                        records[d] = new List<TransactionCsv>();
                    }

                    records[d].Add(new TransactionCsv()
                    {
                        DateTime = DateTime.Parse(d + " " + h.ToString().PadLeft(2, '0') + ":" + m.ToString().PadLeft(2, '0') + ":" + s.ToString()),
                        Type = "TX",
                        Period = csv.GetField<string>(2).Trim(),
                        Price = csv.GetField<double>(4),
                        Volume = csv.GetField<int>(5),
                    });
                }
            }

            return records;
        }

        public bool ToMinPriceCsv(string file, string periods, int min = 5, string type = "TX")
        {
            var records = this.GetTransactionCsvs(file, periods, type);

            foreach (KeyValuePair<string, List<TransactionCsv>> item in records)
            {
                var vv = new MinPriceCsv()
                {
                    Open = item.Value[0].Price,
                    High = item.Value[0].Price,
                    Low = item.Value[0].Price,
                    Close = item.Value[0].Price,
                    DateTime = item.Value[0].DateTime.AddSeconds(-item.Value[0].DateTime.Second)
                };

                vv.DateTime = vv.DateTime.AddMinutes(-(item.Value[0].DateTime.Minute % 5));
                var data = new List<MinPriceCsv>();

                foreach (var v in item.Value)
                {
                    if (v.DateTime.Subtract(vv.DateTime).TotalSeconds >= 300)
                    {
                        vv.Volume = vv.Volume / 2;
                        data.Add(vv);

                        vv = new MinPriceCsv()
                        {
                            Open = v.Price,
                            High = v.Price,
                            Low = v.Price,
                            Close = v.Price,
                            DateTime = v.DateTime.AddSeconds(-v.DateTime.Second)
                        };

                        vv.DateTime = vv.DateTime.AddMinutes(-(v.DateTime.Minute % 5));
                    }


                    vv.Volume += v.Volume;
                    vv.Close = v.Price;

                    if (v.Price > vv.High)
                    {
                        vv.High = v.Price;
                    }

                    if (v.Price < vv.Low)
                    {
                        vv.Low = v.Price;
                    }
                }

                vv.Volume = vv.Volume / 2;
                data.Add(vv);

                CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture);

                var dir = this.sourceDir + "\\price\\" + min.ToString() + "min\\" + periods;

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var f = dir + "\\" + item.Key;

                if (item.Key == records.Keys.ToList()[0])
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
                csvw.WriteRecords(data);
            }

            return true;
        }

        public bool ToCostCsv(string file, DateTime dateTime)
        {
            var csvs = new Dictionary<string, List<CostCsv>>();
            var data = new Dictionary<string, Dictionary<string, List<TransactionCsv>>>();

            foreach (KeyValuePair<string, List<TransactionCsv>> item in this.GetTransactionCsvs(file, null, "TX"))
            {
                data[item.Key] = new Dictionary<string, List<TransactionCsv>>();

                foreach (var v in item.Value)
                {
                    if (!data[item.Key].ContainsKey(v.Period))
                    {
                        data[item.Key][v.Period] = new List<TransactionCsv>();
                    }

                    data[item.Key][v.Period].Add(v);
                }
            }

            foreach (KeyValuePair<string, Dictionary<string, List<TransactionCsv>>> row in data)
            {
                foreach (KeyValuePair<string, List<TransactionCsv>> period in row.Value)
                {
                    if (period.Key.Length != 6)
                    {
                        continue;
                    }

                    if (!csvs.ContainsKey(period.Key))
                    {
                        csvs[period.Key] = new List<CostCsv>();
                    }

                    var times = new DateTime[3, 2] {
                        {
                            DateTime.Parse(row.Key+" 08:45:00"),
                            DateTime.Parse(row.Key+" 08:45:00").AddHours(5),
                        },
                        {
                            DateTime.Parse(row.Key+" 09:00:00"),
                            DateTime.Parse(row.Key+" 13:30:00"),
                        },
                        {
                            DateTime.Parse(row.Key+" 15:00:00"),
                            DateTime.Parse(row.Key+" 15:00:00").AddHours(14),
                        },
                    };

                    for (int i = 0; i < times.Length / 2; i++)
                    {
                        int volume = 0;
                        Int64 money = 0;

                        foreach (var value in period.Value)
                        {
                            if (value.DateTime >= times[i, 0] && value.DateTime <= times[i, 1])
                            {
                                volume += value.Volume;
                                money += (int)(value.Volume * value.Price);
                            }
                        }

                        csvs[period.Key].Add(new CostCsv()
                        {
                            Date = DateTime.Parse(row.Key),
                            Period = period.Key,
                            StartDateTime = times[i, 0],
                            EndDateTime = times[i, 1],
                            Volume = volume,
                            Money = money,
                        });
                    }
                }
            }

            foreach (KeyValuePair<string, List<CostCsv>> item in csvs)
            {
                var dir = this.sourceDir + "\\cost\\" + item.Key;

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using var writer = new StreamWriter(dir + "\\" + dateTime.ToString("yyyy-MM-dd") + ".csv", false, Encoding.UTF8);
                using var csvw = new CsvWriter(writer, new CsvConfiguration(CultureInfo.CurrentCulture));
                csvw.WriteRecords(item.Value);
            }

            return true;
        }

        public void LoadPriceFiles(string period)
        {
            if (this.PriceDates.ContainsKey(period))
            {
                return;
            }

            this.PriceDates[period] = new List<string>();
            var hash = new HashSet<string>();
            var dir = this.priceDir + period;
            foreach (var file in (new DirectoryInfo(dir)).GetFiles("*.csv"))
            {
                hash.Add(file.Name.Substring(0, 10));
            }

            this.PriceDates[period] = hash.ToList();
            this.PriceDates[period].Sort((x, y) => -x.CompareTo(y));
        }

        public List<MinPriceCsv> Get5MinK(DateTime date, string period)
        {
            if (!this.data.ContainsKey(period))
            {
                this.data[period] = new SortedList<DateTime, List<MinPriceCsv>>();
            }

            if (this.data[period].ContainsKey(date))
            {
                return this.data[period][date];
            }
            else
            {
                this.data[period][date] = new List<MinPriceCsv>();
            }

            this.LoadPriceFiles(period);

            if (!this.PriceDates[period].Contains(date.ToString("yyyy-MM-dd")))
            {
                return new List<MinPriceCsv>();
            }

            var files = new string[] {
                this.priceDir+"\\"+period+"\\"+date.ToString("yyyy-MM-dd")+".csv",
                this.priceDir+"\\"+period+"\\"+date.ToString("yyyy-MM-dd")+"-night.csv",
            };

            foreach (var path in files)
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                using var reader = new StreamReader(path);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                {
                    foreach (var item in csv.GetRecords<MinPriceCsv>())
                    {
                        this.data[period][date].Add(item);
                    }
                }
            }

            return this.data[period][date];
        }

        public List<MinPriceCsv> Get5MinKRange(string period, DateTime startDate, DateTime endDate)
        {
            var date = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            var value = new List<MinPriceCsv>();

            for (int i = 0; i <= Math.Round((endDate - startDate).TotalDays); i++)
            {
                value.AddRange(this.Get5MinK(date.AddDays(i), period));
            }

            return value;
        }

        public List<CostCsv> GetCost(DateTime date, string period)
        {
            if (!this.costData.ContainsKey(period))
            {
                this.costData[period] = new SortedList<DateTime, List<CostCsv>>();
            }

            if (!this.costData[period].ContainsKey(date))
            {
                this.costData[period][date] = new List<CostCsv>();

                var file = this.costDir + "\\" + period + "\\" + date.ToString("yyyy-MM-dd") + ".csv";

                if (!File.Exists(file))
                {
                    return new List<CostCsv>();
                }

                using var reader = new StreamReader(file);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                {
                    foreach (var item in csv.GetRecords<CostCsv>())
                    {
                        this.costData[period][date].Add(item);
                    }
                }
            }

            return this.costData[period][date];
        }

        public Dictionary<string, List<CostCsv>> GetCostRange(string period, DateTime startDate, DateTime endDate)
        {
            var date = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            var value = new Dictionary<string, List<CostCsv>>();

            for (int i = 0; i <= Math.Round((endDate - startDate).TotalDays); i++)
            {
                var cost = this.GetCost(date.AddDays(i), period);

                if (cost.Count > 0)
                {
                    value[date.AddDays(i).ToString("yyyy-MM-dd")] = cost;
                }
            }

            return value;
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
            var dir = this.priceDir + "\\" + period;
            if (Directory.Exists(dir))
            {
                foreach (var file in (new DirectoryInfo(this.priceDir + "\\" + period)).GetFiles("*.csv"))
                {
                    files.Add(file.Name);
                }

                files.Sort((x, y) => -x.CompareTo(y));
            }

            return files;
        }
    }
}
