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
            var data = this.Transaction.Get5MinK(this.selectPeriodBox.Text, date, this.mode.Text == "日");
            var futures = this.Futures.Get5MinK(date, this.mode.Text == "日");
            var startDateTime = new DateTime(date.Year, date.Month, date.Day, 8, 45, 0);
            var endDateTime = startDateTime.AddHours(5);
            var watchs = new List<Watch>();

            date = new DateTime(
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
                    if (call.DateTime < startDateTime || call.DateTime > endDateTime || call.DateTime > date)
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
                    if (put.DateTime < startDateTime || put.DateTime > endDateTime || put.DateTime > date)
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
}
