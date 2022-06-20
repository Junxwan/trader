using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace trader.OPS
{
    /// <summary>
    /// _5minKView.xaml 的互動邏輯
    /// </summary>
    public partial class _5minKView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Manage Manage
        {
            get
            {
                return (Manage)GetValue(ManageProperty);
            }
            set
            {
                SetValue(ManageProperty, value);
            }
        }

        public Transaction Transaction
        {
            get
            {
                return (Transaction)GetValue(TransactionProperty);
            }
            set
            {
                SetValue(TransactionProperty, value);
            }
        }

        public Futures.Transaction Futures
        {
            get
            {
                return (Futures.Transaction)GetValue(FuturesProperty);
            }
            set
            {
                SetValue(FuturesProperty, value);
            }
        }

        private Tick? Tick;

        private Calendar Calendar;

        private List<Watch> data = new List<Watch>();

        public List<Watch> Data
        {
            get => data;
            set
            {
                data = value;
                OnPropertyChanged("Data");
            }
        }

        private List<FuturesWatch> fData = new List<FuturesWatch>();

        public List<FuturesWatch> FData
        {
            get => fData;
            set
            {
                fData = value;
                OnPropertyChanged("FData");
            }
        }

        private List<VerticalHeader> verticalHeaders = new List<VerticalHeader>();

        public List<VerticalHeader> VerticalHeaders
        {
            get => verticalHeaders;
            set
            {
                verticalHeaders = value;
            }
        }

        private List<PriceDiffWatchData> priceDiffWatchData = new List<PriceDiffWatchData>();
        public List<PriceDiffWatchData> PriceDiffWatchData
        {
            get => priceDiffWatchData;
            set
            {
                priceDiffWatchData = value;
                OnPropertyChanged("PriceDiffWatchData");
            }
        }

        public static readonly DependencyProperty TransactionProperty =
            DependencyProperty.Register("Transaction", typeof(Transaction), typeof(_5minKView));


        public static readonly DependencyProperty ManageProperty =
            DependencyProperty.Register("Manage", typeof(Manage), typeof(_5minKView));


        public static readonly DependencyProperty FuturesProperty =
            DependencyProperty.Register("Futures", typeof(Futures.Transaction), typeof(_5minKView));

        private SortedDictionary<int, Csv.Tick> nowData;

        public _5minKView()
        {
            var v = new VerticalHeader();
            v.BCall = "空差";
            v.SPut = "多差";
            this.verticalHeaders.Add(v);

            InitializeComponent();

            this.datePicker.SelectedDate = DateTime.Now;
            this.diffPrices.ItemsSource = new int[] { 50, 100 };
            this.diffPrices.SelectedIndex = 0;

            this.selectCP.ItemsSource = new string[] { "call", "put" };
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //盤中資料
        public void Load(DateTime date, DateTime time)
        {
            var period = this.selectPeriodBox.Text;
            var watchs = new List<Watch>();
            SortedDictionary<int, Csv.Tick> colsePrice = new SortedDictionary<int, Csv.Tick>();
            var startDateTime = new DateTime(date.Year, date.Month, date.Day, 8, 45, 0);
            var endDateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);

            // 日盤
            if (time.Hour >= 8 && time.Hour <= 13)
            {
                colsePrice = this.Tick.GetTime(period, new DateTime(date.Year, date.Month, date.Day, 4, 55, 0));
            }
            // 隔日夜盤
            else if (time.Hour >= 0 && time.Hour <= 4)
            {
                startDateTime = startDateTime.AddDays(-1);
                startDateTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 15, 0, 0);
                var y = date.AddDays(-1);
                colsePrice = this.Tick.GetTime(period, new DateTime(y.Year, y.Month, y.Day, 13, 40, 0));
            }
            // 當日夜盤
            else
            {
                startDateTime = new DateTime(date.Year, date.Month, date.Day, 15, 0, 0);
                colsePrice = this.Tick.GetTime(period, new DateTime(date.Year, date.Month, date.Day, 13, 40, 0));
            }

            var data = this.Tick.GetRange(period, startDateTime, endDateTime);
            var volume = new Dictionary<int, Dictionary<string, int>>();

            foreach (var item in data)
            {
                foreach (var v in item.Values)
                {
                    if (!volume.ContainsKey(v.Price))
                    {
                        volume[v.Price] = new Dictionary<string, int>();
                        volume[v.Price]["call"] = 0;
                        volume[v.Price]["put"] = 0;
                    }

                    volume[v.Price]["call"] += v.Call_Volume;
                    volume[v.Price]["put"] += v.Put_Volume;
                }
            }

            nowData = data.Last();

            foreach (var k in volume.Keys)
            {
                watchs.Add(new Watch()
                {
                    CTotal = volume[k]["call"],
                    PTotal = volume[k]["put"],
                    CChange = Math.Round(nowData[k].Call - (colsePrice.ContainsKey(k) ? colsePrice[k].Call : 0), 2),
                    PChange = Math.Round(nowData[k].Put - (colsePrice.ContainsKey(k) ? colsePrice[k].Put : 0), 2),
                    CPrice = nowData[k].Call,
                    PPrice = nowData[k].Put,
                    Performance = k,
                });
            }

            this.Data = watchs;
            this.Performances.ItemsSource = volume.Keys;

            if (Calendar == null)
            {
                this.Calendar = new Calendar(Transaction.sourceDir);
            }

            var fk = volume.Keys.ToList()[0];
            var fw = new FuturesWatch()
            {
                Name = "台指期",
                Month = this.Calendar.GetFutures(period),
                Price = nowData[fk].Futures,
                Change = colsePrice.ContainsKey(fk) ? nowData[fk].Futures - colsePrice[fk].Futures : 0,
                Increase = colsePrice.ContainsKey(fk) ? Math.Round(((nowData[fk].Futures - colsePrice[fk].Futures) / colsePrice[fk].Futures) * 100, 2) : 0,
            };

            this.FData = new List<FuturesWatch>() { fw };

            this.LoadVerticalWatchData(nowData);
        }

        // 價差單
        public void LoadVerticalWatchData(SortedDictionary<int, Csv.Tick> data)
        {
            var diff = Convert.ToInt32(this.diffPrices.SelectedValue);
            var watch = new List<PriceDiffWatchData>();

            foreach (var key in data.Keys)
            {
                var k = key + diff;
                if (!data.ContainsKey(k))
                {
                    continue;
                }

                watch.Add(new PriceDiffWatchData()
                {
                    Performance = key,
                    C_SB = Math.Round(data[key].Call - data[k].Call, 2),
                    P_SB = Math.Round(data[k].Put - data[key].Put, 2),
                });
            }

            this.PriceDiffWatchData = watch;
        }

        private void Button_Run_Click(object sender, RoutedEventArgs e)
        {
            var date = DateTime.Parse(this.datePicker.Text);
            var time = new DateTime(
                date.Year, date.Month, date.Day,
                Convert.ToInt32(this.hour.Text.Trim()),
                Convert.ToInt32(this.minute.Text.Trim()),
                0
                );

            if (this.Tick == null)
            {
                this.Tick = new Tick(Transaction, Futures);
            }

            this.Load(date, time);
        }


        private void DiffPrices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (nowData != null)
            {
                this.LoadVerticalWatchData(nowData);
            }
        }

        private void Performances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.selectCP.SelectedValue == null || this.Performances.SelectedValue == null)
            {
                return;
            }

            var cp = this.selectCP.SelectedValue.ToString();
            var date = DateTime.Parse(this.datePicker.Text);
            var data = this.Transaction.Get5MinKRange(this.selectPeriodBox.Text, date.AddDays(-7), date)[this.Performances.SelectedValue.ToString()];
            var t = TimeSpan.FromMinutes(5);
            OHLC[] prices = new OHLC[data[cp].Count];

            for (int i = 0; i < data[cp].Count; i++)
            {
                prices[i] = new OHLC(data[cp][i].Open, data[cp][i].High, data[cp][i].Low, data[cp][i].Close, data[cp][i].DateTime, t, data[cp][i].Volume);
            }

            var candlePlot = this.KChart.Plot.AddCandlesticks(prices);
            candlePlot.ColorDown = ColorTranslator.FromHtml("#FFFFFF");
            candlePlot.ColorUp = ColorTranslator.FromHtml("#FF0000");
            this.KChart.Plot.Style(figureBackground: System.Drawing.Color.Black, dataBackground: System.Drawing.Color.Black);
            this.KChart.Plot.XAxis.TickLabelStyle(color: System.Drawing.Color.White);
            this.KChart.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            this.KChart.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));
            this.KChart.Plot.YAxis.TickLabelStyle(color: System.Drawing.Color.White);
            this.KChart.Plot.YAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            this.KChart.Plot.YAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));
            this.KChart.Plot.XAxis.DateTimeFormat(true);
            this.KChart.Refresh();
        }
    }

    public class Watch
    {
        //C總量
        public int CTotal { get; set; }

        //P總量
        public int PTotal { get; set; }

        //C漲跌
        public double CChange { get; set; }

        //P漲跌
        public double PChange { get; set; }

        //C成交價
        public double CPrice { get; set; }

        //P成交價
        public double PPrice { get; set; }

        //履約價
        public int Performance { get; set; }
    }

    public class FuturesWatch
    {
        public string Name { get; set; }

        public string Month { get; set; }

        public double Price { get; set; }

        public double Change { get; set; }

        public double Increase { get; set; }
    }

    public class VerticalHeader
    {
        public string BCall { get; set; }

        public string SPut { get; set; }
    }

    public class PriceDiffWatchData
    {
        // C空差
        public double C_SB { get; set; }

        // P多差
        public double P_SB { get; set; }

        public int Performance { get; set; }
    }
}
