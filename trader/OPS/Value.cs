using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using trader.Futures;

namespace trader.OPS
{
    public class Value
    {
        public Transaction Transaction;

        public Futures.Transaction FTransaction;

        public Calendar Calendar;

        private string dir;

        public Value(Transaction Transaction, Futures.Transaction FTransaction)
        {
            this.Transaction = Transaction;
            this.FTransaction = FTransaction;
            this.Calendar = new Calendar(Transaction.sourceDir);
            this.dir = Transaction.sourceDir + "\\value";
        }

        public bool ToCsv(string period, DateTime dateTime)
        {
            var fd = this.FTransaction.Get5MinK(dateTime, this.Calendar.GetFutures(period));
            var opd = this.Transaction.Get5MinK(period, dateTime);
            var csv = new List<Csv.Value>();

            CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture);

            if ((fd.Count != 228) || (fd.Count != opd[opd.Keys[opd.Keys.Count / 2]]["call"].Count))
            {
                throw new Exception("op跟期貨資料不一致 op:" + opd[opd.Keys[opd.Keys.Count / 2]]["call"].Count + " 期貨:" + fd.Count);
            }

            var pdir = this.dir + "\\" + period;

            if (!Directory.Exists(pdir))
            {
                Directory.CreateDirectory(pdir);
            }

            var startTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
            if (this.Calendar.GetFirstDate(period) == dateTime)
            {
                startTime = startTime.AddHours(8).AddMinutes(45);
            }

            var fdk = new Dictionary<DateTime, Double>();
            foreach (var item in fd)
            {
                if (item.DateTime >= startTime)
                {
                    fdk[item.DateTime] = item.Close;
                }
            }

            foreach (var k in opd.Keys)
            {
                var ik = Convert.ToInt32(k);
                var opk = new Dictionary<DateTime, Dictionary<string, Csv.MinPrice>>();

                csv.Clear();

                foreach (var cp in new string[] { "call", "put" })
                {
                    foreach (var v in opd[k][cp])
                    {
                        if (!opk.ContainsKey(v.DateTime))
                        {
                            opk[v.DateTime] = new Dictionary<string, Csv.MinPrice>();
                        }


                        opk[v.DateTime][cp] = v;
                    }
                }

                foreach (var f in fdk)
                {
                    Double c = 0;
                    Double p = 0;

                    if (opk.ContainsKey(f.Key))
                    {
                        c = opk[f.Key].ContainsKey("call") ? opk[f.Key]["call"].Close : 0;
                        p = opk[f.Key].ContainsKey("put") ? opk[f.Key]["put"].Close : 0;
                    }

                    csv.Add(new Csv.Value()
                    {
                        Time = f.Key,
                        Futures = f.Value,
                        Call = c,
                        Put = p,
                        Period = period,
                        Price = ik,
                    });
                }

                var pvdir = pdir + "\\" + k;

                if (!Directory.Exists(pvdir))
                {
                    Directory.CreateDirectory(pvdir);
                }

                var file = pvdir + "\\" + dateTime.ToString("yyyy-MM-dd") + ".csv";

                if (!File.Exists(file))
                {
                    FileStream fs = File.Create(file);
                    fs.Close();
                    fs.Dispose();
                }

                using var writer = new StreamWriter(file, false, Encoding.UTF8);
                using var csvw = new CsvWriter(writer, csvConfig);
                csvw.WriteRecords(csv);
            }

            return true;
        }
    }
}
