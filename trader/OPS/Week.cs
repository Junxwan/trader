using CsvHelper;
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
    // 某一週別OP未平倉
    public class Week
    {
        private SortedList<DateTime, FuturesCsv> futures;

        //未平倉
        public List<Day> Value { get; private set; }

        //有開倉的履約價
        public int[] PerformancePrices { get; private set; } = new int[] { };

        //週別
        private string period;

        public Week(string period, FileInfo[] files, SortedList<DateTime, FuturesCsv> futures)
        {
            this.period = period;
            this.futures = futures;
            Value = new List<Day>();

            foreach (var file in files)
            {
                using var reader = new StreamReader(file.FullName);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var date = DateTime.Parse(file.Name.Substring(0, file.Name.IndexOf('.')));
                var opd = new Day(period, futures[date].WSettlement > 0 ? futures[date].WSettlement : futures[date].Close, futures[date].Change, date, csv.GetRecords<Csv.OP>());
                Value.Add(opd);

                if (this.PerformancePrices.Length < opd.PerformancePrices.Length)
                {
                    this.PerformancePrices = opd.PerformancePrices;
                }
            }

            foreach (var item in Value)
            {
                item.Fill(this.PerformancePrices);
            }

            Value.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            for (int i = 1; i < Value.Count; i++)
            {
                Value[i].SetChange(Value[i - 1]);
            }
        }

        public (List<Day>, int[]) Page(int performance = 0)
        {
            var page = new List<Day>(this.Value);

            if (page.Count < 6)
            {
                for (int i = 0; i < 6 - this.Value.Count; i++)
                {
                    page.Add(new Day(this.period, 0, this.PerformancePrices));
                }

                page.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            }
            else
            {
                page = page.GetRange(page.Count - 6, 6);
            }

            int maxCount = 36;
            int max = 0;
            int min = 0;

            if (performance == 0)
            {
                max = this.PerformancePrices.Length - 1;
                min = 0;

                if (this.PerformancePrices.Length > maxCount)
                {
                    var p = page[page.Count - 1].Price;
                    if (p % 100 > 50)
                    {
                        p += 100 - (p % 100);
                    }
                    else
                    {
                        p -= p % 100;
                    }

                    var index = Array.IndexOf(this.PerformancePrices, p);
                    max = (index + maxCount / 2) >= this.PerformancePrices.Length ? this.PerformancePrices.Length - 1 : (index + maxCount / 2);
                    min = (index - maxCount / 2) < 0 ? 0 : index - maxCount / 2;
                }
            }
            else
            {
                var index = Array.IndexOf(this.PerformancePrices, performance);
                min = index - maxCount / 2;
                max = index + maxCount / 2;
            }

            for (int i = 0; i < page.Count; i++)
            {
                page[i] = page[i].SetRange(this.PerformancePrices[min], this.PerformancePrices[max]);
            }

            return (page, this.PerformancePrices[min..(max + 1)]);
        }
    }

}
