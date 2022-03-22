using System;
using System.Windows;
using System.Windows.Forms;
using trader.OPS;
using trader.Futures;
using IniParser;
using IniParser.Model;
using System.IO;

namespace trader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OPManage OP;

        private Price Futures;

        private Config config;

        public MainWindow()
        {
            InitializeComponent();
            this.config = new Config();
            this.DataPath.Text = this.config.GetData("Path");
            this.Futures = new Price(this.DataPath.Text + "\\futures");
            this.OP = new OPManage(this.DataPath.Text, this.Futures);
            this.Date.SelectedDate = DateTime.Now;
        }

        //打開OP未平倉
        private void Button_OPWindow_Click(object sender, RoutedEventArgs e)
        {
            var op = new OPWindow();
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
    }
}
