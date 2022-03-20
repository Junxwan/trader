using System;
using System.Collections.Generic;
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
    public partial class MainWindow : Window
    {
        public List<OPDView> OPCAll { get; set; }

        public List<OPDView> OPPUT { get; set; }

        public int[] Prices { get; set; } = new int[] { };

        public int Price { get; set; }

        public MainWindow()
        {
            this.OPCAll = new List<OPDView>();
            this.OPPUT = new List<OPDView>();

            var mag = new OPManage("G:\\我的雲端硬碟\\金融\\data\\op\\");
            var opw = mag.Get("202203");

            this.Prices = opw.PerformancePrices;
            this.Price = 18000;

            foreach (var item in opw.Page())
            {
                this.OPCAll.Add(new OPDView(item, OP.Type.CALL));
                this.OPPUT.Add(new OPDView(item, OP.Type.PUT));
            }

            this.DataContext = this;

            InitializeComponent();
        }
    }
}
