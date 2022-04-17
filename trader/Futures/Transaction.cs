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

        private SortedList<DateTime, List<MinPriceCsv>> data;

        public Transaction(string sourceDir)
        {
            this.sourceDir = sourceDir + "\\futures";
            this.data = new SortedList<DateTime, List<MinPriceCsv>>();
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

                vv.DateTime = vv.DateTime.AddMinutes(5 - (item.Value[0].DateTime.Minute % 5));
                var data = new List<MinPriceCsv>();

                foreach (var v in item.Value)
                {
                    if (vv.DateTime <= v.DateTime)
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

                        vv.DateTime = vv.DateTime.AddMinutes(5 - (v.DateTime.Minute % 5));
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

        public List<MinPriceCsv> Get5MinK(DateTime date, string period)
        {
            if (this.data.ContainsKey(date))
            {
                return this.data[date];
            }

            var dir = this.sourceDir + "\\price\\5min\\" + period;
            var files = new string[] { };
            var fn = "";
            foreach (var file in (new DirectoryInfo(dir)).GetFiles("*.csv"))
            {
                if (file.Name.StartsWith(date.ToString("yyyy-MM-dd")))
                {
                    files = new string[] {
                        file.DirectoryName+"\\"+fn.Replace("-night.csv","")+".csv",
                        file.DirectoryName+"\\"+fn.Replace("-night.csv","")+"-night.csv",
                        file.DirectoryName+"\\"+date.ToString("yyyy-MM-dd")+".csv",
                        file.DirectoryName+"\\"+date.ToString("yyyy-MM-dd")+"-night.csv",
                    };
                }
                else
                {
                    fn = file.Name;
                }
            }

            foreach (var path in files)
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                var k = DateTime.Parse(Path.GetFileName(path).Substring(0, 10));

                if (!this.data.ContainsKey(k))
                {
                    this.data[k] = new List<MinPriceCsv>();
                }
                else if (this.data[k].Count > 200)
                {
                    continue;
                }

                using var reader = new StreamReader(path);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                {
                    foreach (var item in csv.GetRecords<MinPriceCsv>())
                    {
                        this.data[k].Add(item);
                    }
                }
            }

            return this.data[date];
        }

        public List<MinPriceCsv> PrevGet5MinK(DateTime date, string period)
        {
            var dates = this.data.Keys.ToArray();
            var index = Array.IndexOf(dates, date);
            if (index == 0)
            {
                var dir = this.sourceDir + "\\price\\5min\\" + period;
                var yn = "";
                foreach (var file in (new DirectoryInfo(dir)).GetFiles("*.csv"))
                {
                    if (file.Name.Substring(0, 10) == date.ToString("yyyy-MM-dd"))
                    {
                        if (yn != "")
                        {
                            date = DateTime.Parse(yn.Substring(0, 10));

                        }

                        break;
                    }
                    else
                    {
                        yn = file.Name;
                    }
                }

                this.Get5MinK(date, period);
                dates = this.data.Keys.ToArray();
                index = Array.IndexOf(dates, date);
            }

            return this.data[dates[index - 1]];
        }
    }
}
