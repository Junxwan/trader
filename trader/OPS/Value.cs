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
            CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture);
            var csv = new List<Csv.Value>();
            var fd = this.FTransaction.Get5MinK(dateTime, this.Calendar.GetFutures(period));
            var opd = this.Transaction.Get5MinK(period, dateTime);
            DateTime startTime = dateTime;
            DateTime endTime = dateTime.AddDays(1);

            if (this.Calendar.GetFirstDate(period) == dateTime)
            {
                startTime = dateTime.AddHours(8).AddMinutes(45);
            }

            if (this.Calendar.GetEndDate(period) == dateTime)
            {
                endTime = dateTime.AddHours(13).AddMinutes(45);
            }

            var fdk = new Dictionary<DateTime, Double>();
            foreach (var item in fd)
            {
                if (item.DateTime >= startTime && item.DateTime <= endTime)
                {
                    fdk[item.DateTime] = item.Close;
                }
            }

            var pdir = this.dir + "\\" + period;

            if (!Directory.Exists(pdir))
            {
                Directory.CreateDirectory(pdir);
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
                    int cv = 0;
                    int pv = 0;

                    if (opk.ContainsKey(f.Key))
                    {
                        if (opk[f.Key].ContainsKey("call"))
                        {
                            c = opk[f.Key]["call"].Close;
                            cv = opk[f.Key]["call"].Volume;
                        }

                        if (opk[f.Key].ContainsKey("put"))
                        {
                            p = opk[f.Key]["put"].Close;
                            pv = opk[f.Key]["put"].Volume;
                        }
                    }

                    csv.Add(new Csv.Value()
                    {
                        Time = f.Key,
                        Futures = f.Value,
                        Call = c,
                        Put = p,
                        Call_Volume = cv,
                        Put_Volume = pv,
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

        public void Get(string period, DateTime startTime)
        {

        }
    }
}
