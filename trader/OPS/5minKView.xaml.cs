﻿using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public static readonly DependencyProperty TransactionProperty =
            DependencyProperty.Register("Transaction", typeof(Transaction), typeof(_5minKView));

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

        public static readonly DependencyProperty ManageProperty =
            DependencyProperty.Register("Manage", typeof(Manage), typeof(_5minKView));

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

        public static readonly DependencyProperty FuturesProperty =
            DependencyProperty.Register("Futures", typeof(Futures.Transaction), typeof(_5minKView));

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

        private List<VerticalWatchData> verticalWatchData = new List<VerticalWatchData>();
        public List<VerticalWatchData> VerticalWatchData
        {
            get => verticalWatchData;
            set
            {
                verticalWatchData = value;
                OnPropertyChanged("VerticalWatchData");
            }
        }

        public _5minKView()
        {
            var v = new VerticalHeader();
            v.BCall = "空差";
            v.SPut = "多差";
            this.verticalHeaders.Add(v);

            InitializeComponent();

            this.datePicker.SelectedDate = DateTime.Now;
            this.diffPrices.ItemsSource = new int[] { 50, 100, 150, 200 };
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

        public void Load(DateTime date, DateTime time)
        {
            var startDateTime = new DateTime(date.Year, date.Month, date.Day, 8, 45, 0);
            var endDateTime = startDateTime.AddHours(5);
            bool IsDayPlate = (startDateTime > time || time > endDateTime) ? false : true;

            var data = this.Transaction.Get5MinK(this.selectPeriodBox.Text, date);
            var watchs = new List<Watch>();

            if (!IsDayPlate)
            {
                var pData = this.Transaction.PrevGet5MinK(this.selectPeriodBox.Text, date);
                foreach (KeyValuePair<string, Dictionary<string, List<Csv.MinPrice>>> row in data)
                {
                    if (pData.ContainsKey(row.Key))
                    {
                        pData[row.Key]["call"].AddRange(row.Value["call"]);
                        pData[row.Key]["put"].AddRange(row.Value["put"]);
                    }
                    else
                    {
                        pData[row.Key] = row.Value;
                    }
                }

                if (pData.First().Value["call"].Count > 0)
                {
                    date = DateTime.Parse(pData.First().Value["call"][0].DateTime.ToString("yyyy-MM-dd"));
                }
                else
                {
                    date = DateTime.Parse(pData.First().Value["put"][0].DateTime.ToString("yyyy-MM-dd"));
                }

                startDateTime = new DateTime(date.Year, date.Month, date.Day, 15, 0, 0);
                endDateTime = startDateTime.AddHours(14);

                data = pData;
            }

            var colsePrice = this.Transaction.PrevGetLast5MinK(this.selectPeriodBox.Text, date, IsDayPlate);
            var performances = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, List<Csv.MinPrice>>> row in data)
            {
                int CTotal = 0;
                int PTotal = 0;
                double CChange = 0;
                double PChange = 0;
                double CPrice = 0;
                double PPrice = 0;

                foreach (var call in row.Value["call"])
                {
                    if (call.DateTime < startDateTime || call.DateTime > endDateTime || call.DateTime > time)
                    {
                        continue;
                    }

                    CChange = Math.Round(call.Close - (colsePrice.ContainsKey(row.Key) ? colsePrice[row.Key]["call"].Close : 0), 2);
                    CPrice = call.Close;
                    CTotal += call.Volume;
                }

                foreach (var put in row.Value["put"])
                {
                    if (put.DateTime < startDateTime || put.DateTime > endDateTime || put.DateTime > time)
                    {
                        continue;
                    }

                    PChange = Math.Round(put.Close - (colsePrice.ContainsKey(row.Key) ? colsePrice[row.Key]["put"].Close : 0), 2);
                    PPrice = put.Close;
                    PTotal += put.Volume;
                }

                watchs.Add(new Watch()
                {
                    CTotal = CTotal,
                    PTotal = PTotal,
                    CChange = CChange,
                    PChange = PChange,
                    CPrice = CPrice,
                    PPrice = PPrice,
                    Performance = row.Key,
                });

                performances.Add(row.Key);
            }

            this.Data = watchs;

            this.Performances.ItemsSource = performances;

            this.LoadVerticalWatchData();
        }

        public void LoadFuturesWatch(DateTime date, DateTime time)
        {
            var fw = new FuturesWatch();
            fw.Name = "台指期";
            fw.Month = this.selectPeriodBox.Text.Substring(4, 2);
            var futures = this.Futures.Get5MinK(date, this.selectPeriodBox.Text);
            List<Futures.MinPriceCsv> prevData;
            DateTime CloseTime;

            foreach (var item in futures)
            {
                if (item.DateTime <= time)
                {
                    fw.Price = item.Close;
                }
            }

            prevData = this.Futures.PrevGet5MinK(date, this.selectPeriodBox.Text);
            CloseTime = new DateTime(prevData[0].DateTime.Year, prevData[0].DateTime.Month, prevData[0].DateTime.Day, 13, 45, 0);

            foreach (var item in prevData)
            {
                if (item.DateTime == CloseTime)
                {
                    fw.Change = fw.Price - item.Close;
                    fw.Increase = Math.Round((fw.Change / item.Close) * 100, 2);
                    break;
                }
            }

            this.FData = new List<FuturesWatch>() { fw };
        }

        public void LoadVerticalWatchData()
        {
            var d = new SortedList<int, Dictionary<string, double>>();
            var watch = new List<VerticalWatchData>();
            var diff = Convert.ToInt32(this.diffPrices.SelectedValue);

            foreach (var item in this.Data)
            {
                var v = new Dictionary<string, double>();
                v["call"] = item.CPrice;
                v["put"] = item.PPrice;
                d[Convert.ToInt32(item.Performance)] = v;
            }

            foreach (var performance in d.Keys.ToArray())
            {
                double call = 0;
                double put = 0;

                if (d.ContainsKey(performance + diff))
                {
                    call = Math.Round(d[performance]["call"] - d[performance + diff]["call"], 2);
                    put = Math.Round(d[performance + diff]["put"] - d[performance]["put"], 2);
                }

                if (call > 0 || put > 0)
                {
                    watch.Add(new VerticalWatchData()
                    {
                        Call = call,
                        Put = put,
                        CPerformance = performance,
                        PPerformance = performance + diff,
                    });
                }
            }

            this.VerticalWatchData = watch;
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

            this.Load(date, time);
            this.LoadFuturesWatch(date, time);
        }

        private void DiffPrices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.LoadVerticalWatchData();
        }

        private void Performances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.selectCP.SelectedValue == null || this.Performances.SelectedValue == null)
            {
                return;
            }

            var date = DateTime.Parse(this.datePicker.Text);
            var data = this.Transaction.PrevGet5MinK(this.selectPeriodBox.Text, date);

            foreach (KeyValuePair<string, Dictionary<string, List<Csv.MinPrice>>> row in this.Transaction.Get5MinK(this.selectPeriodBox.Text, date))
            {
                if (data.ContainsKey(row.Key))
                {
                    data[row.Key]["call"].AddRange(row.Value["call"]);
                    data[row.Key]["put"].AddRange(row.Value["put"]);
                }
                else
                {
                    data[row.Key] = row.Value;
                }
            }

            string performance = this.Performances.SelectedValue.ToString();
            string prevPerformance = (Convert.ToInt32(performance) - Convert.ToInt32(this.diffPrices.Text)).ToString();
            string nextPerformance = (Convert.ToInt32(performance) + Convert.ToInt32(this.diffPrices.Text)).ToString();
            var prices = new Dictionary<string, SortedList<DateTime, double>>();
            var cp = this.selectCP.SelectedValue.ToString();
            var title = "";
            int buyIndex = 0;
            List<Csv.MinPrice> sell;
            List<Csv.MinPrice> buy;
            prices[cp] = new SortedList<DateTime, double>();

            if (cp == "call")
            {
                sell = data[performance][cp];
                buy = data[nextPerformance][cp];
                title = "買權空差 Sell " + performance + " Buy " + nextPerformance + " 歷史價差";
            }
            else
            {
                buy = data[performance][cp];
                sell = data[prevPerformance][cp];
                title = "賣權多差 Sell " + prevPerformance + " Buy " + performance + " 歷史價差";
            }

            for (int i = 0; i < sell.Count; i++)
            {
                var sellDateTime = sell[i].DateTime;

                for (int ci = buyIndex; ci < buy.Count; ci++)
                {
                    buyIndex = ci;

                    if (buy[ci].DateTime.Equals(sellDateTime))
                    {
                        prices[cp][sellDateTime] = Math.Round(sell[i].Close - buy[ci].Close, 2);
                        break;
                    }
                    else if (buy[ci].DateTime > sellDateTime)
                    {
                        break;
                    }
                }
            }

            var fPrices = new List<double>();
            var fData = this.Futures.PrevGet5MinK(date, this.selectPeriodBox.Text);
            fData.AddRange(this.Futures.Get5MinK(date, this.selectPeriodBox.Text));
            var fIndex = 0;
            foreach (var item in prices[cp].Keys)
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
            double[] positions = DataGen.Consecutive(prices[cp].Count);
            double[] dates = prices[cp].Keys.Select(x => x.ToOADate()).ToArray();
            var opChart = this.DiffPricesChart.Plot.AddScatter(dates, prices[cp].Values.ToArray(), label: cp, color: System.Drawing.Color.Red);
            opChart.YAxisIndex = 0;
            opChart.XAxisIndex = 0;

            var futuresChart = this.DiffPricesChart.Plot.AddScatter(dates, fPrices.ToArray(), label: "台指", color: System.Drawing.Color.Green);
            futuresChart.YAxisIndex = 1;
            futuresChart.XAxisIndex = 0;
            this.DiffPricesChart.Plot.YAxis2.Ticks(true);

            this.DiffPricesChart.Plot.XAxis.ManualTickSpacing(12);
            this.DiffPricesChart.Plot.XAxis.DateTimeFormat(true);
            this.DiffPricesChart.Plot.Title(title);
            this.DiffPricesChart.Refresh();
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
        public string Performance { get; set; }
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

    public class VerticalWatchData
    {
        public double Call { get; set; }

        public double Put { get; set; }

        public int Performance { get; set; }

        public int CPerformance { get; set; }

        public int PPerformance { get; set; }
    }
}
