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

namespace trader.Page
{
    /// <summary>
    /// FuturesCostAvgWindow.xaml 的互動邏輯
    /// </summary>
    public partial class FuturesCostAvg : Window
    {
        private Config config;

        private Price Futures;

        public FuturesCostAvg()
        {
            InitializeComponent();

            this.config = new Config();
            this.Futures = new Price(this.config.GetData("Path"));

            this.avgComboBox.ItemsSource = new int[] { 5, 20, 60, 120, 240 };
            this.avgComboBox.SelectedIndex = 0;
        }

        private void runBtn_Click(object sender, RoutedEventArgs e)
        {
            var data = this.Futures.All().Reverse().ToList();
            int max = Convert.ToInt32(this.avgComboBox.SelectedValue.ToString());
            DateTime date = this.datePicker.SelectedDate.Value;
            var prices = new List<int>();

            prices.Add(Convert.ToInt32(this.closePrice.Text));

            foreach (KeyValuePair<DateTime, Futures.FuturesCsv> item in data)
            {
                if (date <= item.Key)
                {
                    continue;
                }

                prices.Add(item.Value.Close);

                if (prices.Count == max)
                {
                    break;
                }
            }

            var avg = prices.Sum() / max;

            MessageBox.Show(avg.ToString());
        }
    }
}
