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
        public List<OPDView> CALL { get; set; } = new List<OPDView>();

        public List<OPDView> PUT { get; set; } = new List<OPDView>();

        public int[] Prices { set; get; } = new int[] { };

        public PageData() { }

        public PageData(OPW opw)
        {
            List<OPD> data;
            var _call = new List<OPDView>();
            var _put = new List<OPDView>();
            var prices = new int[] { };
            (data, prices) = opw.Page();

            foreach (var item in data)
            {
                _call.Add(new OPDView(item, OP.Type.CALL));
            }

            data.Reverse();

            foreach (var item in data)
            {
                _put.Add(new OPDView(item, OP.Type.PUT));
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

        public OPManage Manage
        {
            get
            {
                return (OPManage)GetValue(ManageProperty);
            }
            set
            {
                SetValue(ManageProperty, value);
            }
        }

        public static readonly DependencyProperty ManageProperty =
            DependencyProperty.Register("Manage", typeof(OPManage), typeof(View));

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Page = new PageData(Manage.Get((string)this.selectPeriodBox.SelectedValue));
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
