using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.OPS
{
    //某天OP未平倉
    public class OPD
    {
        //週別
        public string Period { get => this.period; }
        private readonly string period;

        //日期
        public DateTime DateTime { get; private set; }

        //有開倉的履約價
        private readonly SortedSet<int> performancePrices;
        public int[] PerformancePrices
        {
            get
            {
                int[] p = new int[performancePrices.Count];
                performancePrices.CopyTo(p);
                return p;
            }
        }

        //指數
        public int Price { get; private set; }

        //指數變化
        public int PriceChange { get; private set; }

        //C OP未平倉
        public List<OP> Calls { get; private set; }

        //P OP未平倉
        public List<OP> Puts { get; private set; }

        //未平倉增加最多的call/put履約價
        public int CallMaxAddChangePerformancePrices { get; private set; } = 0;
        public int PutMaxAddChangePerformancePrices { get; private set; } = 0;

        //未平倉減少最多的call/put履約價
        public int CallMaxSubChangePerformancePrices { get; private set; } = 0;
        public int PutMaxSubChangePerformancePrices { get; private set; } = 0;

        public OPD(string period, int price, int PriceChange, DateTime dateTime, IEnumerable<OPCsv> csv)
        {
            this.period = period;
            this.DateTime = dateTime;
            this.Price = price;
            this.PriceChange = PriceChange;
            this.performancePrices = new SortedSet<int>();
            this.Calls = new List<OP>();
            this.Puts = new List<OP>();

            foreach (var row in csv)
            {
                var call = new OP(row.C, row.Price, OP.Type.CALL);
                var put = new OP(row.P, row.Price, OP.Type.PUT);
                call.SetPrice(price);
                put.SetPrice(price);
                this.Calls.Add(call);
                this.Puts.Add(put);
                this.performancePrices.Add(row.Price);
            }
        }

        public OPD(string period, int price, int[] performancePrices)
        {
            this.period = period;
            this.performancePrices = new SortedSet<int>();
            this.Price = price;
            this.Calls = new List<OP>();
            this.Puts = new List<OP>();
            foreach (var p in performancePrices)
            {
                this.Calls.Add(new OP(0, p, OP.Type.CALL));
                this.Puts.Add(new OP(0, p, OP.Type.PUT));
            }
        }

        //填充履約價(空倉)
        public void Fill(int[] performancePrices)
        {
            foreach (var p in performancePrices)
            {
                if (this.performancePrices.Contains(p))
                {
                    continue;
                }

                var call = new OP(0, p, OP.Type.CALL);
                var put = new OP(0, p, OP.Type.PUT);
                call.SetPrice(this.Price);
                put.SetPrice(this.Price);

                this.Calls.Add(call);
                this.Puts.Add(put);
            }

            this.Calls.Sort((x, y) => x.PerformancePrice.CompareTo(y.PerformancePrice));
            this.Puts.Sort((x, y) => x.PerformancePrice.CompareTo(y.PerformancePrice));
        }

        //設定OP未平倉變化量
        public void SetChange(OPD opd)
        {
            foreach (var x in this.Calls)
            {
                foreach (var y in opd.Calls)
                {
                    if (x.PerformancePrice == y.PerformancePrice)
                    {
                        x.SetChange(y);
                        continue;
                    }
                }
            }

            foreach (var x in this.Puts)
            {
                foreach (var y in opd.Puts)
                {
                    if (x.PerformancePrice == y.PerformancePrice)
                    {
                        x.SetChange(y);
                        continue;
                    }
                }
            }

            var c = new List<OP>(this.Calls);
            c.Sort((x, y) => x.Change.CompareTo(y.Change));
            this.CallMaxSubChangePerformancePrices = c[0].PerformancePrice;
            this.CallMaxAddChangePerformancePrices = c[this.Calls.Count - 1].PerformancePrice;

            c = new List<OP>(this.Puts);
            c.Sort((x, y) => x.Change.CompareTo(y.Change));
            this.PutMaxSubChangePerformancePrices = c[0].PerformancePrice;
            this.PutMaxAddChangePerformancePrices = c[this.Calls.Count - 1].PerformancePrice;

            foreach (var x in this.Calls)
            {
                if (x.PerformancePrice == this.CallMaxSubChangePerformancePrices)
                {
                    x.IsMaxSubChangeForDay = true;
                    continue;
                }

                if (x.PerformancePrice == this.CallMaxAddChangePerformancePrices)
                {
                    x.IsMaxAddChangeForDay = true;
                    continue;
                }
            }

            foreach (var x in this.Puts)
            {
                if (x.PerformancePrice == this.PutMaxSubChangePerformancePrices)
                {
                    x.IsMaxSubChangeForDay = true;
                    continue;
                }

                if (x.PerformancePrice == this.PutMaxAddChangePerformancePrices)
                {
                    x.IsMaxAddChangeForDay = true;
                    continue;
                }
            }
        }

        //日期
        public string Date()
        {
            return this.DateTime.ToString("MM-dd");
        }

        //週
        public string Week()
        {
            return this.DateTime.ToString("ddd");
        }

        public OPD SetRange(int min = 0, int max = 9999)
        {
            var opd = this.MemberwiseClone() as OPD;
            (opd.Calls, opd.Puts) = opd.GetRange(min, max);
            return opd;
        }

        public (List<OP>, List<OP>) GetRange(int min = 0, int max = 9999)
        {
            var calls = new List<OP>();
            var puts = new List<OP>();
            foreach (var item in this.Calls)
            {
                if (item.PerformancePrice >= min && item.PerformancePrice <= max)
                {
                    calls.Add(item);
                }
            }

            foreach (var item in this.Puts)
            {
                if (item.PerformancePrice >= min && item.PerformancePrice <= max)
                {
                    puts.Add(item);
                }
            }

            return (calls, puts);
        }
    }
}
