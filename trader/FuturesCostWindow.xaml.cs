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

namespace trader
{
    /// <summary>
    /// FuturesCostWindow.xaml 的互動邏輯
    /// </summary>
    public partial class FuturesCostWindow : Window
    {
        public Futures.Transaction Transaction { get; set; }

        public Futures.Price Price { get; set; }

        public FuturesCostWindow()
        {
            var dataPath = (new Config()).GetData("Path");
            Transaction = new Futures.Transaction(dataPath);
            Price = new Futures.Price(dataPath);

            InitializeComponent();
        }
    }
}
