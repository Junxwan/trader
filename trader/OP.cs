using CsvHelper;
using CsvHelper.Configuration.Attributes;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace trader
{
    //管理OP
    public class OPManage
    {
        private readonly string sourceDir;

        private SortedList<string, DirectoryInfo> periodDirs = new();

        private readonly SortedList<string, OPW> ops;

        public OPManage(string sourceDir)
        {
            this.sourceDir = sourceDir;
            this.ops = new SortedList<string, OPW>();
            this.LoadDirectory();
        }

        public OPW Get(string period)
        {
            FileInfo[] files = this.periodDirs[period].GetFiles("*.csv");
            Array.Sort(files, (f1, f2) => f2.Name.CompareTo(f1.Name));

            if (!ops.ContainsKey(period))
            {
                ops[period] = new OPW(period, files[0..7]);
            }

            return ops[period];
        }

        private void LoadDirectory()
        {
            foreach (string path in Directory.GetDirectories(sourceDir))
            {
                var info = new DirectoryInfo(path);
                this.periodDirs.Add(info.Name, info);
            }
        }

        //下載台指OP未平倉
        public bool Download(DateTime datetime)
        {
            var client = new RestClient("https://www.taifex.com.tw");
            var request = new RestRequest("/cht/3/optDataDown", Method.POST);
            string date = datetime.ToString("yyyy/MM/dd");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("down_type", "1");
            request.AddParameter("commodity_id", "TXO");
            request.AddParameter("commodity_id2", "");
            request.AddParameter("queryStartDate", date);
            request.AddParameter("queryEndDate", date);

            RestResponse resp = (RestResponse)client.Execute(request);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var result = (Encoding.GetEncoding(950)).GetString(resp.RawBytes);
            using var csv = new CsvReader(new StringReader(result), CultureInfo.InvariantCulture);
            return TaifexOPToCsv(csv.GetRecords<TaifexOPCsv>(), this.sourceDir);
        }

        //整理期交所每日台指OP行情csv資料 
        private static bool TaifexOPToCsv(IEnumerable<TaifexOPCsv> csv, string sourceDir)
        {
            var ops = new Dictionary<string, List<TaifexOPCsv>>();

            foreach (var row in csv)
            {
                if (!row.IsUse())
                {
                    continue;
                }

                if (!ops.ContainsKey(row.Period))
                {
                    ops[row.Period] = new List<TaifexOPCsv>();
                }

                ops[row.Period].Add(row);
            }

            foreach (KeyValuePair<string, List<TaifexOPCsv>> entry in ops)
            {
                var cp = new SortedDictionary<double, OPCsv>();
                foreach (TaifexOPCsv op in entry.Value)
                {
                    OPCsv v;
                    int total = Convert.ToInt32(op.Total);

                    if (!cp.ContainsKey(op.Price))
                    {
                        v = new OPCsv() { Price = (int)op.Price };
                        cp[op.Price] = v;
                    }
                    else
                    {
                        v = cp[op.Price];
                    }

                    if (op.CP == "C")
                    {
                        v.C = total;
                    }
                    else
                    {
                        v.P = total;
                    }
                }

                var list = new List<OPCsv>();
                foreach (var item in cp.Values)
                {
                    list.Add(item);
                }

                string dir = sourceDir + entry.Key;
                string date = entry.Value[0].Date.ToString("yyyy-MM-dd");
                string file = dir + "\\" + date + ".csv";

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(file))
                {
                    FileStream f = File.Create(file);
                    f.Close();
                    f.Dispose();
                }

                using var writer = new StreamWriter(file, false, Encoding.UTF8);
                using var csvw = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csvw.WriteRecords(list);
            }

            return true;
        }
    }

    // 某一週別OP未平倉
    public class OPW
    {
        //未平倉
        public List<OPD> Value { get; private set; }

        //有開倉的履約價
        private readonly int[] performancePrices = new int[] { };

        //週別
        private string period;

        public OPW(string period, FileInfo[] files)
        {
            this.period = period;
            Value = new List<OPD>();

            foreach (var file in files)
            {
                using var reader = new StreamReader(file.FullName);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var opd = new OPD(period, DateTime.Parse(file.Name.Substring(0, file.Name.IndexOf('.'))), csv.GetRecords<OPCsv>(), 0);
                Value.Add(opd);

                if (this.performancePrices.Length < opd.PerformancePrices.Length)
                {
                    this.performancePrices = opd.PerformancePrices;
                }
            }

            foreach (var item in Value)
            {
                item.Fill(this.performancePrices);
            }

            Value.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            for (int i = 1; i < Value.Count; i++)
            {
                Value[i].SetChange(Value[i - 1]);
            }
        }
    }

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
        private readonly int price;

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

        //日期(顯示)
        public string DateText { get; private set; }

        //指數(顯示)
        public string PriceText { get; private set; }

        public OPD(string period, DateTime dateTime, IEnumerable<OPCsv> csv, int price)
        {
            this.period = period;
            this.DateTime = dateTime;
            this.price = price;
            this.performancePrices = new SortedSet<int>();
            this.Calls = new List<OP>();
            this.Puts = new List<OP>();

            foreach (var row in csv)
            {
                this.Calls.Add(new OP(row.C, row.Price, OP.Type.CALL));
                this.Puts.Add(new OP(row.P, row.Price, OP.Type.PUT));
                this.performancePrices.Add(row.Price);
            }

            this.DateText = this.DateTime.ToString("MM-dd") + "(" + this.DateTime.ToString("ddd") + ")";
            this.PriceText = "0";
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

                this.Calls.Add(new OP(0, p, OP.Type.CALL));
                this.Puts.Add(new OP(0, p, OP.Type.PUT));
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
    }

    //某個履約價的OP未平倉
    public class OP
    {
        public enum Volume
        {
            //萬口 ro 最大量
            MAX,

            //7000口以上
            Notice,

            //7000口以下
            Normal
        }

        public enum Type
        {
            //賣權
            PUT,
            //買權
            CALL,
        }

        //跟上次比變化值
        public int Change { get { return this.change; } }
        private int change;

        //未平倉
        public int Total { get { return this.total; } }
        private readonly int total;

        //履約價
        public int PerformancePrice { get { return this.performancePrice; } }
        private readonly int performancePrice;

        //買賣權
        private readonly Type cp;

        //是否履約
        private bool isPerformance;

        public OP(int total, int performancePrice, Type cp)
        {
            this.cp = cp;
            this.total = total;
            this.performancePrice = performancePrice;
        }

        //指數價格
        public void SetPrice(int price)
        {
            this.isPerformance = (price > this.performancePrice);
        }

        //未平倉變化
        public void SetChange(OP op)
        {
            this.change = this.total - op.total;
        }

        //是否履約
        public bool IsPerformance()
        {
            return this.isPerformance;
        }

        //未平倉等級
        public Volume VolumeStatus()
        {
            if (this.total >= 10000)
            {
                return Volume.MAX;
            }

            if (this.total >= 7000)
            {
                return Volume.Notice;
            }

            return Volume.Normal;
        }
    }

    //Converter =========================================================================================

    public class XxxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGridCell cell = (DataGridCell)value;
            OP user = (OP)cell.DataContext;
            int v = 0;
            string p = (string)parameter;

            if (cell.Column.DisplayIndex == 0)
            {
                v = user.Change;
            }
            else if (cell.Column.DisplayIndex == 1)
            {
                v = user.Total;
            }

            switch (p)
            {
                case "sub":
                    return v < 0;
                case "add":
                    return v > 0;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsPerformanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((OP)value).IsPerformance();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //CSV ===============================================================================================

    public class OPCsv
    {
        //合約價
        [Index(0)]
        public int Price { get; set; }

        //call 未平倉
        [Index(1)]
        public int C { get; set; }

        //put 未平倉
        [Index(2)]
        public int P { get; set; }
    }

    //期交所每日台指OP行情 https://www.taifex.com.tw/cht/3/optDailyMarketView csv格式
    public class TaifexOPCsv
    {
        //交易日期
        [Index(0)]
        public DateTime Date { get; set; }

        // 契約
        [Index(1)]
        public string Type { get; set; } = "";

        //到期月份(週別)
        private string period = "";

        [Index(2)]
        public string Period
        {
            get => period;
            set { period = value.Trim(); }
        }

        //履約價
        [Index(3)]
        public double Price { get; set; }

        //買賣權
        private string cp = "";

        [Index(4)]
        public string CP
        {
            get { return cp; }
            set
            {
                if (value == "買權")
                {
                    cp = "C";
                }
                else
                {
                    cp = "P";
                }
            }
        }

        //未沖銷契約數
        [Index(11)]
        public string Total { get; set; } = "0";

        //交易時段
        [Index(17)]
        public string S { get; set; } = "盤後";

        //資料正確性
        public bool IsUse()
        {
            return this.Type == "TXO" && this.S == "一般";
        }
    }
}