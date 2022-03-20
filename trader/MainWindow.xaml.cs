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

        public MainWindow()
        {
            this.OPCAll = new List<OPDView>();

            var mag = new OPManage("G:\\我的雲端硬碟\\金融\\data\\op\\");
            foreach (var item in mag.Get("202203").Page())
            {
                this.OPCAll.Add(new OPDView(item, OP.Type.CALL));
            }

            this.DataContext = this;

            InitializeComponent();
        }
    }
}
