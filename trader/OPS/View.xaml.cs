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
    /// View.xaml 的互動邏輯
    /// </summary>
    public partial class View : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public OPManage Manage
        {
            get
            {
                return (OPManage)GetValue(OPManageProperty);
            }
            set
            {
                SetValue(OPManageProperty, value);
            }
        }

        public static readonly DependencyProperty OPManageProperty =
            DependencyProperty.Register("Manage", typeof(OPManage), typeof(View));

        private List<OPDView> call = new();
        public List<OPDView> CALL
        {
            get => call;
            set
            {
                call = value;
                OnPropertyChanged("CALL");
            }
        }

        private List<OPDView> put = new();
        public List<OPDView> PUT
        {
            get
            {
                return put;
            }
            set
            {
                put = value;
                OnPropertyChanged("PUT");
            }
        }

        //履約價
        private int[] prices = new int[] { };
        public int[] Prices
        {
            get
            {
                return prices;
            }
            set
            {
                prices = value;
                OnPropertyChanged("Prices");
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<OPD> page;
            var _call = new List<OPDView>();
            var _put = new List<OPDView>();

            var opw = Manage.Get((string)this.selectPeriodBox.SelectedValue);
            (page, this.Prices) = opw.Page();

            foreach (var item in page)
            {
                _call.Add(new OPDView(item, OP.Type.CALL));
            }

            page.Reverse();

            foreach (var item in page)
            {
                _put.Add(new OPDView(item, OP.Type.PUT));
            }

            this.CALL = _call;
            this.PUT = _put;
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
            this.CALL = new List<OPDView>();
            this.PUT = new List<OPDView>();

            InitializeComponent();
        }
    }
}
