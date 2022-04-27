using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using trader.OPS;
using trader.Futures;

namespace trader
{
    /// <summary>
    /// OPWindow.xaml 的互動邏輯
    /// </summary>
    public partial class OPWindow : Window
    {
        public Manage OP { get; set; }

        public OPWindow()
        {
            var dataPath = (new Config()).GetData("Path");
            OP = new Manage(dataPath, new Price(dataPath));

            InitializeComponent();
        }
    }
}
