using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace trader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private List<OPDView> oPCAll = new();
        public List<OPDView> OPCAll
        {
            get => oPCAll;
            set
            {
                oPCAll = value;
                OnPropertyChanged("OPCAll");
            }
        }

        private List<OPDView> oPPUT = new();
        public List<OPDView> OPPUT
        {
            get
            {
                return oPPUT;
            }
            set
            {
                oPPUT = value;
                OnPropertyChanged("OPPUT");
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

        private OPManage opm { get; set; }

        public MainWindow()
        {
            opm = new OPManage("G:\\我的雲端硬碟\\金融\\data");
            this.DataContext = this;
            
            InitializeComponent();

            this.selectPeriodBox.ItemsSource = opm.Periods();
        }

        private void SetOP(string period)
        {
            List<OPD> page;
            var _oPCAll = new List<OPDView>();
            var _oPPUT = new List<OPDView>();

            var opw = opm.Get(period);
            (page, this.Prices) = opw.Page();

            foreach (var item in page)
            {
                _oPCAll.Add(new OPDView(item, OP.Type.CALL));
            }

            page.Reverse();

            foreach (var item in page)
            {
                _oPPUT.Add(new OPDView(item, OP.Type.PUT));
            }

            this.OPCAll = _oPCAll;
            this.OPPUT = _oPPUT;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var period = (string)this.selectPeriodBox.SelectedValue;
            SetOP(period);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
