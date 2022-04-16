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
using trader.Futures;
using trader.OPS;

namespace trader
{
    /// <summary>
    /// OP5minKWindow.xaml 的互動邏輯
    /// </summary>
    public partial class OP5minKWindow : Window
    {
        public Manage OP { get; set; }

        public trader.OPS.Transaction Transaction { get; set; }

        public trader.Futures.Transaction Futures { get; set; }

        public OP5minKWindow()
        {
            var dataPath = (new Config()).GetData("Path");
            OP = new Manage(dataPath, new Price(dataPath + "\\futures"));
            Transaction = new trader.OPS.Transaction(dataPath);
            Futures = new trader.Futures.Transaction(dataPath);

            InitializeComponent();
        }
    }
}
