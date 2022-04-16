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

        public _5minKView()
        {
            InitializeComponent();

            this.datePicker.SelectedDate = DateTime.Now;
            this.mode.ItemsSource = new string[] { "日", "夜" };
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Button_Run_Click(object sender, RoutedEventArgs e)
        {
            var date = DateTime.Parse(this.datePicker.Text);
            var IsDayPlate = this.mode.Text == "日";
            var data = this.Transaction.Get5MinK(this.selectPeriodBox.Text, date, IsDayPlate);
            var startDateTime = new DateTime(date.Year, date.Month, date.Day, 8, 45, 0);
            var endDateTime = startDateTime.AddHours(5);
            var watchs = new List<Watch>();

            var time = new DateTime(
                date.Year, date.Month, date.Day,
                Convert.ToInt32(this.hour.Text.Trim()),
                Convert.ToInt32(this.minute.Text.Trim()),
                0
                );

            foreach (KeyValuePair<string, Dictionary<string, List<Csv.MinPrice>>> row in data)
            {
                int CTotal = 0;
                int PTotal = 0;
                double CChange = 0;
                double PChange = 0;
                double CPrice = 0;
                double PPrice = 0;
                Csv.MinPrice? COpen = null;
                Csv.MinPrice? POpen = null;

                foreach (var call in row.Value["call"])
                {
                    if (call.DateTime < startDateTime || call.DateTime > endDateTime || call.DateTime > time)
                    {
                        continue;
                    }

                    if (COpen == null)
                    {
                        COpen = call;
                    }

                    CChange = Math.Round(call.Close - COpen.Open, 2);
                    CPrice = call.Close;
                    CTotal += call.Volume;
                }

                foreach (var put in row.Value["put"])
                {
                    if (put.DateTime < startDateTime || put.DateTime > endDateTime || put.DateTime > time)
                    {
                        continue;
                    }

                    if (POpen == null)
                    {
                        POpen = put;
                    }

                    PChange = Math.Round(put.Close - POpen.Open, 2);
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
            }

            this.Data = watchs;

            var fw = new FuturesWatch();
            fw.Name = "台指期";
            fw.Month = this.selectPeriodBox.Text.Substring(4, 2);
            var futures = this.Futures.Get5MinK(date, this.selectPeriodBox.Text);

            foreach (var item in futures)
            {
                if (item.DateTime <= time)
                {
                    fw.Price = item.Close;
                }
            }

            if (IsDayPlate)
            {
                var prevData = this.Futures.PrevGet5MinK(date, this.selectPeriodBox.Text);
                var t = new DateTime(prevData[0].DateTime.Year, prevData[0].DateTime.Month, prevData[0].DateTime.Day, 13, 45, 0);
                foreach (var item in prevData)
                {
                    if (item.DateTime == t)
                    {
                        fw.Change = fw.Price - item.Close;
                        fw.Increase = Math.Round((fw.Change / item.Close) * 100, 2);
                        break;
                    }
                }
            }

            this.FData = new List<FuturesWatch>() { fw };
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
}
