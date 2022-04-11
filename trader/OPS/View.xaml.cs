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
    public class PageData
    {
        public List<DayView> CALL { get; set; } = new List<DayView>();

        public List<DayView> PUT { get; set; } = new List<DayView>();

        public int[] Prices { set; get; } = new int[] { };

        public PageData() { }

        public PageData(Week opw, int performance = 0)
        {
            List<Day> data;
            var _call = new List<DayView>();
            var _put = new List<DayView>();
            var prices = new int[] { };
            (data, prices) = opw.Page(performance);

            foreach (var item in data)
            {
                _call.Add(new DayView(item, OP.Type.CALL));
            }

            data.Reverse();

            foreach (var item in data)
            {
                _put.Add(new DayView(item, OP.Type.PUT));
            }

            this.CALL = _call;
            this.PUT = _put;
            this.Prices = prices;
        }
    }

    /// <summary>
    /// View.xaml 的互動邏輯
    /// </summary>
    public partial class View : UserControl, INotifyPropertyChanged
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
            DependencyProperty.Register("Manage", typeof(Manage), typeof(View));

        private void ComboBox_SelectionPeriodsChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Page = new PageData(Manage.Get((string)this.selectPeriodBox.SelectedValue));
            this.selectPerformanceBox.ItemsSource = this.Page.Prices;
        }

        private void ComboBox_SelectionPerformanceChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Page = new PageData(Manage.Get((string)this.selectPeriodBox.SelectedValue), (int)this.selectPerformanceBox.SelectedValue);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public View()
        {
            InitializeComponent();
        }
    }
}
