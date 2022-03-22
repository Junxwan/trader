using System;
using System.Windows;
using System.Windows.Forms;
using trader.OPS;
using trader.Futures;

namespace trader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OPManage OP;

        private Price Futures;

        public MainWindow()
        {
            InitializeComponent();
            this.OP = new OPManage(this.DataPath.Text);
            this.Futures = new Price(this.DataPath.Text + "\\futures");
            this.Date.SelectedDate = DateTime.Now;
        }

        private void Button_OPWindow_Click(object sender, RoutedEventArgs e)
        {
            var op = new OPWindow();
            op.Show();
        }

        private void Button_OpenDir_Click(object sender, RoutedEventArgs e)
        {
            var folder = new FolderBrowserDialog();
            var result = folder.ShowDialog();
            if (result.ToString() != "OK")
            {
                return;
            }

            this.DataPath.Text = folder.SelectedPath;
        }

        private void Button_DownloadOP_Click(object sender, RoutedEventArgs e)
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

        private void Button_DownloadOPFutures_Click(object sender, RoutedEventArgs e)
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
    }
}
