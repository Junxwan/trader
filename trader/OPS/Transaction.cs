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

        public Transaction(string sourceDir)
        {
            this.sourceDir = sourceDir + "\\op";
        }

        public bool ToMinPriceCsv(string file, string[] periods, int min = 5, string type = "TXO")
        {
            var records = new Dictionary<string, Dictionary<int, Dictionary<string, List<TransactionCsv>>>>();

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

                    var record = new TransactionCsv
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
                        records.Add(record.Period, new Dictionary<int, Dictionary<string, List<TransactionCsv>>>());
                    }

                    if (!records[record.Period].ContainsKey(record.Performance))
                    {
                        var pcp = new Dictionary<string, List<TransactionCsv>>();
                        pcp.Add("C", new List<TransactionCsv>());
                        pcp.Add("P", new List<TransactionCsv>());
                        records[record.Period].Add(record.Performance, pcp);
                    }

                    records[record.Period][record.Performance][record.Cp].Add(record);
                }
            }

            foreach (KeyValuePair<string, Dictionary<int, Dictionary<string, List<TransactionCsv>>>> row in records)
            {
                var dir = this.sourceDir + "\\price\\" + min.ToString() + "min\\" + row.Key;
                Dictionary<string, List<MinPriceCsv>> minData;

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                foreach (KeyValuePair<int, Dictionary<string, List<TransactionCsv>>> prow in row.Value)
                {
                    var pdir = dir + "\\" + prow.Key;

                    if (!Directory.Exists(pdir))
                    {
                        Directory.CreateDirectory(pdir);
                    }

                    foreach (KeyValuePair<string, List<TransactionCsv>> cp in prow.Value)
                    {
                        if (cp.Value.Count == 0)
                        {
                            continue;
                        }

                        cp.Value.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));

                        minData = new Dictionary<string, List<MinPriceCsv>>();
                        var vv = new MinPriceCsv()
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
                            if (vv.DateTime <= item.DateTime || index == cp.Value.Count-1)
                            {
                                vv.Volume = vv.Volume / 2;
                                var date = vv.DateTime.ToString("yyyy-MM-dd");

                                if (!minData.ContainsKey(date))
                                {
                                    minData[date] = new List<MinPriceCsv>();
                                }

                                minData[date].Add(vv);
                                vv = new MinPriceCsv()
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

                        foreach (KeyValuePair<string, List<MinPriceCsv>> cdata in minData)
                        {
                            var f = pdir + "\\" + cdata.Key + "_" + cp.Key + ".csv";

                            if (!File.Exists(f))
                            {
                                FileStream fs = File.Create(f);
                                fs.Close();
                                fs.Dispose();

                                csvConfig.HasHeaderRecord = true;
                            }
                            else
                            {
                                csvConfig.HasHeaderRecord = false;
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
    }
}
