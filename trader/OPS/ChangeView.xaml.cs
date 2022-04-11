using ScottPlot;
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
    /// ChangeView.xaml 的互動邏輯
    /// </summary>
    public partial class ChangeView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private PageData page = new PageData();
        public PageData Page
        {
            get => page;
            set
            {
                page = value;
                OnPropertyChanged("Page");
            }
        }

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
            DependencyProperty.Register("Manage", typeof(Manage), typeof(ChangeView));

        private void ComboBox_SelectionPeriodsChanged(object sender, SelectionChangedEventArgs e)
        {
            var week = Manage.Get((string)this.selectPeriodBox.SelectedValue);
            this.datePicker.SelectedDate = week.Value[week.Value.Count - 1].DateTime;
        }

        private void ComboBox_SelectionPerformanceChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Page = new PageData(Manage.Get((string)this.selectPeriodBox.SelectedValue), (int)this.selectPerformanceBox.SelectedValue);
        }

        private void ComboBox_SelectionPerformanceSupportChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = 0;
            foreach (var item in this.pricesDataGrid.Items)
            {
                var row = ((DataGridRow)this.pricesDataGrid.ItemContainerGenerator.ContainerFromIndex(index));
                if (this.selectCallPerformanceSupportBox.SelectedValue != null && (int)item >= (int)this.selectCallPerformanceSupportBox.SelectedValue)
                {
                    row.Background = Brushes.DarkRed;
                }
                else if (this.selectPutPerformanceSupportBox.SelectedValue != null && (int)item <= (int)this.selectPutPerformanceSupportBox.SelectedValue)
                {
                    row.Background = Brushes.DarkGreen;
                }
                else
                {
                    row.Background = Brushes.White;
                }

                index++;
            }
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            int performance = 0;
            if (this.selectPerformanceBox.SelectedValue != null)
            {
                performance = (int)this.selectPerformanceBox.SelectedValue;
            }

            this.Page = new PageData(
                Manage.Get((string)this.selectPeriodBox.SelectedValue),
                performance,
                DateTime.Parse(this.datePicker.Text).ToString("yyyy-MM-dd")
                );

            this.selectPerformanceBox.ItemsSource = this.Page.Prices;
            this.selectCallPerformanceSupportBox.ItemsSource = this.Page.Prices;
            this.selectPutPerformanceSupportBox.ItemsSource = this.Page.Prices;

            DrawCallPutTotalChart();
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void DrawCallPutTotalChart()
        {
            var callTotal = new List<double>();
            var putTotal = new List<double>();

            foreach (var item in this.page.CALL[0].Value)
            {
                var total = 0.0;

                if (callTotal.Count > 0)
                {
                    total = callTotal.Last() + item.Total;
                }
                else
                {
                    total = item.Total;
                }

                callTotal.Add(total);
            }

            foreach (var item in this.page.PUT[0].Value)
            {
                var total = 0.0;

                if (putTotal.Count > 0)
                {
                    total = putTotal.Last() + item.Total;
                }
                else
                {
                    total = item.Total;
                }

                putTotal.Add(total);
            }

            putTotal.Reverse();

            CallPutTotalChart.Plot.Clear();
            double[] positions = DataGen.Consecutive(this.Page.Prices.Length);
            CallPutTotalChart.Plot.AddScatter(positions, callTotal.ToArray(), label: "Call", color: System.Drawing.Color.Red);
            CallPutTotalChart.Plot.AddScatter(positions, putTotal.ToArray(), label: "Put", color: System.Drawing.Color.Green);

            string[] labels = this.Page.Prices.Select(x => x.ToString()).ToArray();
            CallPutTotalChart.Plot.XAxis.ManualTickPositions(positions, labels);
            CallPutTotalChart.Plot.Title("Call/Put累積未平倉");
            CallPutTotalChart.Plot.XLabel("履約價");
            CallPutTotalChart.Plot.Title("累積未平倉");
            CallPutTotalChart.Plot.Legend();
            CallPutTotalChart.Plot.Style(ScottPlot.Style.Gray2);
            CallPutTotalChart.Plot.XAxis.Color(System.Drawing.Color.White);
            CallPutTotalChart.Plot.YAxis.Color(System.Drawing.Color.White);
            CallPutTotalChart.Refresh();
        }

        public ChangeView()
        {
            InitializeComponent();
        }
    }
}
