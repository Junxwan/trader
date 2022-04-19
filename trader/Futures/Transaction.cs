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

        private Dictionary<string, SortedList<DateTime, List<MinPriceCsv>>> data;

        public Dictionary<string, List<string>> PriceDates;

        public Transaction(string sourceDir)
        {
            this.sourceDir = sourceDir + "\\futures";
            this.priceDir = this.sourceDir + "\\price\\5min\\";
            this.data = new Dictionary<string, SortedList<DateTime, List<MinPriceCsv>>>();
            this.PriceDates = new Dictionary<string, List<string>>();
        }

        public bool ToMinPriceCsv(string file, string periods, int min = 5, string type = "TX")
        {
            var records = new Dictionary<string, List<TransactionCsv>>();
            using var reader = new StreamReader(file);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    if (csv.GetField(1).Trim() != type || periods != csv.GetField<string>(2).Trim())
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

        public List<MinPriceCsv> PrevGet5MinK(DateTime date, string period)
        {
            this.LoadPriceFiles(period);

            var index = Array.IndexOf(this.PriceDates[period].ToArray(), date.ToString("yyyy-MM-dd"));

            if (index == -1)
            {
                return new List<MinPriceCsv>();
            }

            return this.Get5MinK(DateTime.Parse(this.PriceDates[period][index + 1]), period);
        }
    }
}
