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

namespace trader.Futures
{
    /// <summary>
    /// CostView.xaml 的互動邏輯
    /// </summary>
    public partial class CostView : UserControl, INotifyPropertyChanged
    {
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
            DependencyProperty.Register("Transaction", typeof(Transaction), typeof(CostView));

        public Price Price
        {
            get
            {
                return (Price)GetValue(PriceProperty);
            }
            set
            {
                SetValue(PriceProperty, value);
            }
        }

        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register("Price", typeof(Price), typeof(CostView));

        public event PropertyChangedEventHandler? PropertyChanged;

        private List<CostWatch> watchs = new List<CostWatch>();

        public List<CostWatch> Watchs
        {
            get => watchs;
            set
            {
                watchs = value;
                OnPropertyChanged("Watchs");
            }
        }

        public CostView()
        {
            InitializeComponent();
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SelectionPeriodChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.datePicker.Text == "" || this.selectPeriodBox.SelectedValue == null)
            {
                return;
            }

            var startDateTime = DateTime.Parse(this.datePicker.Text);
            int index = 1;
            double price = 0;
            var data = new List<CostWatch>();

            foreach (var row in this.Transaction.GetCostRange(this.selectPeriodBox.SelectedValue.ToString(), startDateTime, startDateTime.AddDays(35)))
            {
                var startTime = DateTime.Parse(row.Key).AddHours(8).AddMinutes(45);
                var endTime = startTime.AddHours(5);

                foreach (var item in row.Value)
                {
                    if (item.StartDateTime == startTime && item.EndDateTime == endTime)
                    {
                        price += item.AvgCost;
                        break;
                    }
                }

                var c = new CostWatch();
                c.Date = row.Key;
                c.Price = Math.Round(price / index, 0);
                index++;

                data.Add(c);
            }

            if (this.nextNumber.Text != "")
            {
                var c = new CostWatch();
                c.Date = "";
                c.Price = Math.Round((price + Int32.Parse(this.nextNumber.Text)) / index + 1, 0);
                data.Add(c);
            }

            this.Watchs = data;
        }
    }

    public class CostWatch
    {
        public string Date { get; set; } = "";

        public double Price { get; set; } = 0;
    }
}
