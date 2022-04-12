using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        private void ComboBox_SelectionAddChange(object sender, SelectionChangedEventArgs e)
        {
            int index = 0;
            foreach (var item in this.pricesDataGrid.Items)
            {
                var callMin = 0;
                var callMax = 0;
                var putMin = 0;
                var putMax = 0;
                var callRow = ((DataGridRow)this.callDataGrid.ItemContainerGenerator.ContainerFromIndex(index));
                var putRow = ((DataGridRow)this.putDataGrid.ItemContainerGenerator.ContainerFromIndex(index));

                if (this.selectCallAddMin.SelectedValue != null)
                {
                    callMin = (int)this.selectCallAddMin.SelectedValue;
                }

                if (this.selectCallAddMax.SelectedValue != null)
                {
                    callMax = (int)this.selectCallAddMax.SelectedValue;
                }

                if (this.selectPutAddMin.SelectedValue != null)
                {
                    putMin = (int)this.selectPutAddMin.SelectedValue;
                }

                if (this.selectPutAddMax.SelectedValue != null)
                {
                    putMax = (int)this.selectPutAddMax.SelectedValue;
                }

                if (callMin > 0 && callMax > 0)
                {
                    if (callMax >= (int)item && (int)item >= callMin)
                    {
                        callRow.Background = Brushes.Red;
                    }
                    else
                    {
                        callRow.Background = Brushes.White;
                    }
                }

                if (putMin > 0 && putMax > 0)
                {
                    if (putMax >= (int)item && (int)item >= putMin)
                    {
                        putRow.Background = Brushes.Red;
                    }
                    else
                    {
                        putRow.Background = Brushes.White;
                    }
                }

                index++;
            }
        }

        private void ComboBox_SelectionCP(object sender, SelectionChangedEventArgs e)
        {
            var min = this.selectCPMin.SelectedValue != null ? (int)this.selectCPMin.SelectedValue : 0;
            var max = this.selectCPMax.SelectedValue != null ? (int)this.selectCPMax.SelectedValue : 0;
            var cTotal = 0;
            var cAddTotal = 0;
            var pTotal = 0;
            var pAddTotal = 0;

            foreach (var item in this.page.CALL[0].Value)
            {
                if (min <= item.PerformancePrice && max >= item.PerformancePrice)
                {
                    cTotal += item.Total;
                    cAddTotal += item.Change;
                }
            }

            foreach (var item in this.page.PUT[0].Value)
            {
                if (min <= item.PerformancePrice && max >= item.PerformancePrice)
                {
                    pTotal += item.Total;
                    pAddTotal += item.Change;
                }
            }

            this.cpTotalLabel.Content = cTotal.ToString() + "/" + pTotal.ToString() + "/" + (cTotal - pTotal).ToString();
            this.cpChangeTotalLabel.Content = cAddTotal.ToString() + "/" + pAddTotal.ToString() + "/" + (cAddTotal - pAddTotal).ToString();
        }

        private void ComboBox_SelectionPerformanceMaxSupport(object sender, SelectionChangedEventArgs e)
        {
            var call = this.selectCallPerformanceMaxSupport.SelectedValue != null ? (int)this.selectCallPerformanceMaxSupport.SelectedValue : 0;
            var put = this.selectPutPerformanceMaxSupport.SelectedValue != null ? (int)this.selectPutPerformanceMaxSupport.SelectedValue : 0;

            int index = 0;
            foreach (var item in this.pricesDataGrid.Items)
            {
                var row = (DataGridRow)this.pricesDataGrid.ItemContainerGenerator.ContainerFromIndex(index);

                if (row == null)
                {
                    break;
                }

                DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);

                var cell = ((DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(0));

                if (call == (int)item)
                {
                    cell.Foreground = Brushes.Red;
                }
                else if (put == (int)item)
                {
                    cell.Foreground = Brushes.Green;
                }
                else {
                    cell.Foreground = Brushes.White;
                }

                index++;
            }
        }

        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
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

            var cs = new ComboBox[] {
                this.selectPerformanceBox,
                this.selectCallPerformanceSupportBox,
                this.selectPutPerformanceSupportBox,
                this.selectCallAddMin,
                this.selectCallAddMax,
                this.selectPutAddMin,
                this.selectPutAddMax,
                this.selectCPMin,
                this.selectCPMin,
                this.selectCPMax,
                this.selectCPMax,
                this.selectCallPerformanceMaxSupport,
                this.selectPutPerformanceMaxSupport,
            };

            foreach (var item in cs)
            {
                item.SelectedValue = 0;
            }

            this.callPutLine1.DrawTotal(this.page.CALL[0].Value, this.page.PUT[0].Value, this.page.Prices, this.page.CALL[0].Price);
            this.callPutLine2.DrawChange(this.page.CALL[0].Value, this.page.PUT[0].Value, this.page.Prices, this.page.PUT[0].Price);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ChangeView()
        {
            InitializeComponent();
        }
    }
}
