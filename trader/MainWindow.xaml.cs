using System;
using System.Windows;
using System.Windows.Forms;
using trader.OPS;
using trader.Futures;
using trader.Page;
using IniParser;
using IniParser.Model;
using System.IO;
using System.Threading;
using System.IO.Compression;

namespace trader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Manage OP;

        private Price Futures;

        private Config config;

        private Futures.Transaction FTransaction;

        private OPS.Transaction OTransaction;

        private OPS.Value OPValue;

        public MainWindow()
        {
            InitializeComponent();
            this.config = new Config();
            this.DataPath.Text = this.config.GetData("Path");
            this.Futures = new Price(this.DataPath.Text);
            this.OP = new Manage(this.DataPath.Text, this.Futures);
            this.Date.SelectedDate = DateTime.Now;

            opPeriods.ItemsSource = this.OP.Periods;
            opPeriods.SelectedIndex = 0;

            FTransaction = new Futures.Transaction(this.DataPath.Text);
            OTransaction = new OPS.Transaction(this.DataPath.Text);
            OPValue = new Value(OTransaction, FTransaction);

            this.futuresDataFileDataGrid.ItemsSource = FTransaction.GetFiles();
            this.futures5MinDataFileDataGrid.ItemsSource = FTransaction.Get5MinFiles(opPeriods.SelectedValue.ToString());
            this.opDataFileDataGrid.ItemsSource = OTransaction.GetFiles();
        }

        //打開OP未平倉
        private void Button_OPWindow_Click(object sender, RoutedEventArgs e)
        {
            var op = new OPPosition();
            op.Show();
        }

        //打開OP當日未平倉變化
        private void Button_OPChangeWindow_Click(object sender, RoutedEventArgs e)
        {
            var op = new OPChange();
            op.Show();
        }

        //打開OP 5分K
        private void Button_OP5minKWindow_Click(object sender, RoutedEventArgs e)
        {
            var op = new OP5minK();
            op.Show();
        }

        //打開市場期貨總成本
        private void Button_FuturesCostWindow_Click(object sender, RoutedEventArgs e)
        {
            var op = new FuturesCost();
            op.Show();
        }

        //打開計算期貨均線
        private void Button_FuturesCostAvgWindow_Click(object sender, RoutedEventArgs e)
        {
            var op = new FuturesCostAvg();
            op.Show();
        }

        //打開OP權利金
        private void Button_OPValueWindow_Click(object sender, RoutedEventArgs e)
        {
            var op = new OPValue();
            op.Show();
        }

        //選擇資料目錄
        private void Button_OpenDataDir_Click(object sender, RoutedEventArgs e)
        {
            var folder = new FolderBrowserDialog();
            var result = folder.ShowDialog();
            if (result.ToString() != "OK")
            {
                return;
            }

            this.DataPath.Text = folder.SelectedPath;
        }

        //下載OP未平倉
        private void Button_DownloadOPTotal_Click(object sender, RoutedEventArgs e)
        {
            if (this.OP.Download(DateTime.Parse(this.Date.Text)))
            {
                System.Windows.MessageBox.Show("完成");
            }
            else
            {
                System.Windows.MessageBox.Show("失敗");
            }
        }

        //下載台指日K資料
        private void Button_DownloadFutures_Click(object sender, RoutedEventArgs e)
        {
            if (this.Futures.Download(DateTime.Parse(this.Date.Text)))
            {
                System.Windows.MessageBox.Show("完成");
            }
            else
            {
                System.Windows.MessageBox.Show("失敗");
            }
        }

        //保存config
        private void Button_Config_Save_Click(object sender, RoutedEventArgs e)
        {
            this.config.WriteData("Path", this.DataPath.Text);
            System.Windows.MessageBox.Show("完成");
        }

        //產生OP 5分K
        private void Button_OP_TO_5MinK_Click(object sender, RoutedEventArgs e)
        {
            var d = DateTime.Parse(this.Date.Text);
            var date = d.ToString("yyyy_MM_dd");
            var file = this.DataPath.Text + "\\op\\transaction\\OptionsDaily_" + date;
            ZipFile.ExtractToDirectory(file + ".zip", Directory.GetCurrentDirectory());
            (new OPS.Transaction(this.DataPath.Text)).ToMinPriceCsv(d, Directory.GetCurrentDirectory() + "\\OptionsDaily_" + date + ".csv", new string[] { this.opPeriods.Text });
            File.Delete(Directory.GetCurrentDirectory() + "\\OptionsDaily_" + date + ".csv");

            System.Windows.MessageBox.Show("完成");
        }

        //產生台指 5分K
        private void Button_Futures_TO_5MinK_Click(object sender, RoutedEventArgs e)
        {
            var date = DateTime.Parse(this.Date.Text).ToString("yyyy_MM_dd");
            var file = this.DataPath.Text + "\\futures\\transaction\\Daily_" + date;

            if (!File.Exists(Directory.GetCurrentDirectory() + "\\Daily_" + date + ".csv"))
            {
                ZipFile.ExtractToDirectory(file + ".zip", Directory.GetCurrentDirectory());
            }

            (new Futures.Transaction(this.DataPath.Text)).ToMinPriceCsv(Directory.GetCurrentDirectory() + "\\Daily_" + date + ".csv", this.opPeriods.Text);
            File.Delete(Directory.GetCurrentDirectory() + "\\Daily_" + date + ".csv");

            System.Windows.MessageBox.Show("完成");
        }

        //產生台指成本
        private void Button_Futures_TO_Cost_Click(object sender, RoutedEventArgs e)
        {
            var datetime = DateTime.Parse(this.Date.Text);
            var date = datetime.ToString("yyyy_MM_dd");
            var file = this.DataPath.Text + "\\futures\\transaction\\Daily_" + date;

            if (!File.Exists(Directory.GetCurrentDirectory() + "\\Daily_" + date + ".csv"))
            {
                ZipFile.ExtractToDirectory(file + ".zip", Directory.GetCurrentDirectory());
            }

            (new Futures.Transaction(this.DataPath.Text)).ToCostCsv(Directory.GetCurrentDirectory() + "\\Daily_" + date + ".csv", datetime);
            File.Delete(Directory.GetCurrentDirectory() + "\\Daily_" + date + ".csv");

            System.Windows.MessageBox.Show("完成");
        }

        //產生OP權利金
        private void Button_OP_TO_Value_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OPValue.ToCsv(this.opPeriods.Text, DateTime.Parse(this.Date.Text));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }

            System.Windows.MessageBox.Show("完成");
        }
    }
}
