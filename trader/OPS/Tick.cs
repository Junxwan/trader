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
    public class Tick
    {
        public Transaction Transaction;

        public Futures.Transaction FTransaction;

        public Calendar Calendar;

        private string dir;

        private Dictionary<string, Dictionary<string, SortedDictionary<int, List<Csv.Tick>>>> data = new Dictionary<string, Dictionary<string, SortedDictionary<int, List<Csv.Tick>>>>();

        private Dictionary<string, Dictionary<DateTime, SortedDictionary<int, Csv.Tick>>> TickData = new Dictionary<string, Dictionary<DateTime, SortedDictionary<int, Csv.Tick>>>();

        public Tick(Transaction Transaction, Futures.Transaction FTransaction)
        {
            this.Transaction = Transaction;
            this.FTransaction = FTransaction;
            this.Calendar = new Calendar(Transaction.sourceDir);
            this.dir = Transaction.sourceDir + "\\value";
        }

        public bool ToCsv(string period, DateTime dateTime)
        {
            CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture);
            var csv = new List<Csv.Tick>();
            var fd = this.FTransaction.Get5MinK(dateTime, this.Calendar.GetFutures(period));
            var opd = this.Transaction.Get5MinK(period, dateTime);
            var ok = fd[0].Close - fd[0].Close % 100;
            DateTime startTime = opd[ok.ToString()]["put"][0].DateTime;
            DateTime endTime = opd[ok.ToString()]["put"].Last().DateTime;

            var fdk = new Dictionary<DateTime, Double>();
            foreach (var item in fd)
            {
                fdk[item.DateTime] = item.Close;
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

                foreach (var time in opk.Keys)
                {
                    Double c = 0;
                    Double p = 0;
                    int cv = 0;
                    int pv = 0;

                    if (!fdk.ContainsKey(time))
                    {
                        // 日盤有些資料會有最後一盤
                        if (time.Minute == 45)
                        {
                            continue;
                        }

                        // 月結只有到25
                        if (time.Minute >= 30 && this.Calendar.GetEndDate(period).Day == dateTime.Day)
                        {
                            continue;
                        }

                        // 有些假日期指沒有資料 2022/05/03 沒有0000-0455
                        if (opk[time].ContainsKey("put") && opk[time]["put"].Volume == 0)
                        {
                            continue;
                        }
                        if (opk[time].ContainsKey("call") && opk[time]["call"].Volume == 0)
                        {
                            continue;
                        }

                        throw new Exception("缺少" + time.ToString() + "期貨資料");
                    }

                    if (opk[time].ContainsKey("call"))
                    {
                        c = opk[time]["call"].Close;
                        cv = opk[time]["call"].Volume;
                    }

                    if (opk[time].ContainsKey("put"))
                    {
                        p = opk[time]["put"].Close;
                        pv = opk[time]["put"].Volume;
                    }

                    csv.Add(new Csv.Tick()
                    {
                        Time = time,
                        Futures = fdk[time],
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

        public SortedDictionary<int, Csv.Tick> GetTime(string period, DateTime time)
        {
            var sourceDir = this.dir + "\\" + period;
            var date = time.ToString("yyyy-MM-dd");

            if (!this.data.ContainsKey(period))
            {
                this.data[period] = new Dictionary<string, SortedDictionary<int, List<Csv.Tick>>>();
            }

            if (!this.data[period].ContainsKey(date))
            {
                this.data[period][date] = new SortedDictionary<int, List<Csv.Tick>>();

                foreach (var info in (new DirectoryInfo(sourceDir)).GetDirectories())
                {
                    foreach (var f in info.GetFiles())
                    {
                        if (f.Name == date + ".csv")
                        {
                            using var reader = new StreamReader(f.FullName);
                            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                            {
                                this.data[period][date][Convert.ToInt32(info.Name)] = csv.GetRecords<Csv.Tick>().ToList();
                            }
                        }
                    }
                }
            }

            if (!this.TickData.ContainsKey(period))
            {
                this.TickData[period] = new Dictionary<DateTime, SortedDictionary<int, Csv.Tick>>();
            }

            if (!this.TickData[period].ContainsKey(time))
            {
                var t = DateTime.Parse(date);
                for (int i = 0; i < 288; i++)
                {
                    this.TickData[period][t.AddMinutes(i * 5)] = new SortedDictionary<int, Csv.Tick>();
                }

                foreach (var item in this.data[period][date])
                {
                    foreach (var v in item.Value)
                    {
                        this.TickData[period][v.Time][item.Key] = v;
                    }
                }
            }

            return this.TickData[period][time];
        }

        public List<SortedDictionary<int, Csv.Tick>> GetRange(string period, DateTime startDate, DateTime EndDate)
        {
            var data = new List<SortedDictionary<int, Csv.Tick>>();
            for (int i = 0; i <= (EndDate - startDate).TotalMinutes / 5; i++)
            {
                data.Add(this.GetTime(period, startDate.AddMinutes(i * 5)));
            }

            return data;
        }
    }
}
