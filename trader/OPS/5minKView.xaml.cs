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

        private List<PriceDiffWatchData> callPriceDiffWatchData = new List<PriceDiffWatchData>();
        public List<PriceDiffWatchData> CallPriceDiffWatchData
        {
            get => callPriceDiffWatchData;
            set
            {
                callPriceDiffWatchData = value;
                OnPropertyChanged("CallPriceDiffWatchData");
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
                Change = nowData[fk].Futures - colsePrice[fk].Futures,
                Increase = Math.Round(((nowData[fk].Futures - colsePrice[fk].Futures) / colsePrice[fk].Futures) * 100, 2),
            };

            this.FData = new List<FuturesWatch>() { fw };

            this.LoadVerticalWatchData(nowData);
        }

        // 價差單
        public void LoadVerticalWatchData(SortedDictionary<int, Csv.Tick> data)
        {
            var diff = Convert.ToInt32(this.diffPrices.SelectedValue);
            var call = new List<PriceDiffWatchData>();
            var put = new List<PriceDiffWatchData>();

            foreach (var key in data.Keys)
            {
                var k = key + diff;
                if (!data.ContainsKey(k))
                {
                    continue;
                }

                call.Add(new PriceDiffWatchData()
                {
                    Performance = key,
                    SB = Math.Round(data[key].Call - data[k].Call, 2),
                    BS = 0,
                });

                put.Add(new PriceDiffWatchData()
                {
                    Performance = key,
                    SB = Math.Round(data[key].Put - data[k].Put, 2),
                    BS = 0,
                });
            }

            this.CallPriceDiffWatchData = call;
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

        public void LoadDiffPricesChart()
        {
            if (this.selectCP.SelectedValue == null || this.Performances.SelectedValue == null)
            {
                return;
            }

            var date = DateTime.Parse(this.datePicker.Text);
            var fFates = this.Futures.PriceDates[this.selectFuturesPeriodBox.Text];
            var data = this.Transaction.Get5MinKRange(this.selectPeriodBox.Text, date.AddDays(-7), date);

            string performance = this.Performances.SelectedValue.ToString();
            string prevPerformance = (Convert.ToInt32(performance) - Convert.ToInt32(this.diffPrices.Text)).ToString();
            string nextPerformance = (Convert.ToInt32(performance) + Convert.ToInt32(this.diffPrices.Text)).ToString();
            var opPrices = new Dictionary<string, SortedList<DateTime, double>>();
            var cp = this.selectCP.SelectedValue.ToString();
            var title = "";
            int buyIndex = 0;
            List<Csv.MinPrice> sell;
            List<Csv.MinPrice> buy;
            opPrices[cp] = new SortedList<DateTime, double>();

            if (cp == "call")
            {
                sell = data[performance][cp];
                buy = data[nextPerformance][cp];
                title = "買權空差 Sell " + performance + " Buy " + nextPerformance + " 歷史價差";
            }
            else
            {
                sell = data[performance][cp];
                buy = data[prevPerformance][cp];
                title = "賣權多差 Sell " + performance + " Buy " + prevPerformance + " 歷史價差";
            }

            for (int i = 0; i < sell.Count; i++)
            {
                var sellDateTime = sell[i].DateTime;

                for (int ci = buyIndex; ci < buy.Count; ci++)
                {
                    buyIndex = ci;

                    if (buy[ci].DateTime.Equals(sellDateTime))
                    {
                        opPrices[cp][sellDateTime] = Math.Round(sell[i].Close - buy[ci].Close, 2);
                        break;
                    }
                    else if (buy[ci].DateTime > sellDateTime)
                    {
                        break;
                    }
                }
            }

            var fPrices = new List<double>();
            var fData = this.Futures.Get5MinKRange(this.selectFuturesPeriodBox.Text, date.AddDays(-7), date);
            var fIndex = 0;
            foreach (var item in opPrices[cp].Keys)
            {
                for (int i = fIndex; i < fData.Count; i++)
                {
                    fIndex = i;
                    if (fData[i].DateTime.Equals(item))
                    {
                        fPrices.Add(fData[i].Close);
                        break;
                    }
                    else if (fData[i].DateTime > item)
                    {
                        break;
                    }
                }
            }

            this.DiffPricesChart.Plot.Clear();
            double[] positions = DataGen.Consecutive(opPrices[cp].Count);
            double[] dates = opPrices[cp].Keys.Select(x => x.ToOADate()).ToArray();
            var opChart = this.DiffPricesChart.Plot.AddScatter(dates, opPrices[cp].Values.ToArray(), label: cp, color: System.Drawing.Color.Red);
            opChart.YAxisIndex = 0;
            opChart.XAxisIndex = 0;

            var futuresChart = this.DiffPricesChart.Plot.AddScatter(dates, fPrices.ToArray(), label: "台指期", color: System.Drawing.Color.Green);
            futuresChart.YAxisIndex = 1;
            futuresChart.XAxisIndex = 0;

            this.DiffPricesChart.Plot.YAxis2.Ticks(true);
            this.DiffPricesChart.Plot.XAxis.ManualTickSpacing(12);
            this.DiffPricesChart.Plot.XAxis.DateTimeFormat(true);
            this.DiffPricesChart.Plot.Title(title);
            this.DiffPricesChart.Plot.Legend();
            this.DiffPricesChart.Plot.Style(ScottPlot.Style.Gray2);
            this.DiffPricesChart.Plot.YAxis.Color(System.Drawing.Color.White);
            this.DiffPricesChart.Plot.YAxis2.Color(System.Drawing.Color.White);
            this.DiffPricesChart.Plot.XAxis.Color(System.Drawing.Color.White);
            this.DiffPricesChart.Refresh();
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
            this.LoadDiffPricesChart();

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
        //賣高價買低價 C空差 P多差
        public double SB { get; set; }

        //買高價賣低價 C多差 P空差
        public double BS { get; set; }

        public int Performance { get; set; }
    }
}
