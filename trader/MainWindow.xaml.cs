﻿using System;
using System.Windows;
using System.Windows.Forms;
using trader.OPS;
using trader.Futures;
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

            opPeriods.ItemsSource = this.OP.Periods;
            opPeriods.SelectedIndex = 0;

            (new trader.Futures.Transaction(this.DataPath.Text)).ToMinPriceCsv("G:\\我的雲端硬碟\\金融\\data\\futures\\transaction\\Daily_2022_04_08.csv", "202204");
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

        //產生OP 5分K
        private void Button_OP_TO_5MinK_Click(object sender, RoutedEventArgs e)
        {
            var date = DateTime.Parse(this.Date.Text).ToString("yyyy_MM_dd");
            var file = this.DataPath.Text + "\\op\\transaction\\OptionsDaily_" + date;
            ZipFile.ExtractToDirectory(file + ".zip", Directory.GetCurrentDirectory());
            (new trader.OPS.Transaction(this.DataPath.Text)).ToMinPriceCsv(Directory.GetCurrentDirectory() + "\\OptionsDaily_" + date + ".csv", new string[] { this.opPeriods.Text });
            File.Delete(Directory.GetCurrentDirectory() + "\\OptionsDaily_" + date + ".csv");

            System.Windows.MessageBox.Show("完成");
        }
    }
}
