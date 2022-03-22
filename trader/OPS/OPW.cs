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
    public class OPW
    {
        private SortedList<DateTime, FuturesCsv> futures;

        //未平倉
        public List<OPD> Value { get; private set; }

        //有開倉的履約價
        public int[] PerformancePrices { get; private set; } = new int[] { };

        //週別
        private string period;

        public OPW(string period, FileInfo[] files, SortedList<DateTime, FuturesCsv> futures)
        {
            this.period = period;
            this.futures = futures;
            Value = new List<OPD>();

            foreach (var file in files)
            {
                using var reader = new StreamReader(file.FullName);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var date = DateTime.Parse(file.Name.Substring(0, file.Name.IndexOf('.')));
                var opd = new OPD(period, futures[date].Settlement, date, csv.GetRecords<OPCsv>());
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

        public (List<OPD>, int[]) Page()
        {
            var page = new List<OPD>(this.Value);

            if (page.Count < 6)
            {
                for (int i = 0; i < 6 - this.Value.Count; i++)
                {
                    page.Add(new OPD(this.period, 0, this.PerformancePrices));
                }

                page.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            }
            else
            {
                page = page.GetRange(page.Count - 6, 6);
            }

            int maxCount = 38;
            int max = this.PerformancePrices.Length - 1;
            int min = 0;

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

                for (int i = 0; i < page.Count; i++)
                {
                    page[i] = page[i].SetRange(this.PerformancePrices[min], this.PerformancePrices[max]);
                }
            }

            return (page, this.PerformancePrices[min..(max + 1)]);
        }
    }

}
