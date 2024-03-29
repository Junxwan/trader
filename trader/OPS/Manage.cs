﻿using CsvHelper;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using trader.Futures;

namespace trader.OPS
{
    //管理OP
    public class Manage
    {
        //op資料目錄
        private readonly string sourceDir;

        private SortedList<string, DirectoryInfo> periodDirs = new();

        public List<string> Periods { get; private set; }

        public List<string> FuturesPeriods { get; private set; }

        private readonly SortedList<string, Week> ops;

        private Price futures;

        public Manage(string sourceDir, Price futures)
        {
            this.sourceDir = sourceDir + "\\op\\chips";
            this.futures = futures;
            this.ops = new SortedList<string, Week>();
            this.LoadDirectory();
            this.Periods = this.GetPeriods();
            this.FuturesPeriods = this.GetFuturesPeriods();
        }

        private List<string> GetPeriods()
        {
            var v = new List<string>();
            foreach (var item in this.periodDirs.Keys)
            {
                v.Add(item);
            }

            v.Reverse();

            return v;
        }

        private List<string> GetFuturesPeriods()
        {
            var v = new List<string>();
            foreach (var item in this.periodDirs.Keys)
            {
                if (item.Length == 6)
                {
                    v.Add(item);
                }
            }

            v.Reverse();

            return v;
        }

        public Week Get(string period)
        {
            FileInfo[] files = this.periodDirs[period].GetFiles("*.csv");
            Array.Sort(files, (f1, f2) => f2.Name.CompareTo(f1.Name));

            if (!ops.ContainsKey(period))
            {
                if (files.Length < 7)
                {
                    ops[period] = new Week(period, files, this.futures.All());
                }
                else
                {
                    ops[period] = new Week(period, files[0..7], this.futures.All());
                }
            }

            return ops[period];
        }

        private void LoadDirectory()
        {
            foreach (string path in Directory.GetDirectories(sourceDir))
            {
                var info = new DirectoryInfo(path);
                this.periodDirs.Add(info.Name, info);
            }
        }

        //下載台指OP未平倉
        public bool Download(DateTime datetime)
        {
            //六日沒開盤
            if (datetime.DayOfWeek == DayOfWeek.Sunday || datetime.DayOfWeek == DayOfWeek.Saturday)
            {
                return true;
            }

            var client = new RestClient("https://www.taifex.com.tw");
            var request = new RestRequest("/cht/3/optDataDown", Method.POST);
            string date = datetime.ToString("yyyy/MM/dd");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("down_type", "1");
            request.AddParameter("commodity_id", "TXO");
            request.AddParameter("commodity_id2", "");
            request.AddParameter("queryStartDate", date);
            request.AddParameter("queryEndDate", date);

            RestResponse resp = (RestResponse)client.Execute(request);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var result = (Encoding.GetEncoding(950)).GetString(resp.RawBytes);
            using var csv = new CsvReader(new StringReader(result), CultureInfo.InvariantCulture);
            return TaifexOPToCsv(csv.GetRecords<Csv.TaifexOP>(), this.sourceDir);
        }

        public bool DownloadPrice(DateTime datetime)
        {
            return true;
        }

        //整理期交所每日台指OP行情csv資料 
        private static bool TaifexOPToCsv(IEnumerable<Csv.TaifexOP> csv, string sourceDir)
        {
            var ops = new Dictionary<string, List<Csv.TaifexOP>>();

            foreach (var row in csv)
            {
                if (!row.IsUse())
                {
                    continue;
                }

                if (!ops.ContainsKey(row.Period))
                {
                    ops[row.Period] = new List<Csv.TaifexOP>();
                }

                ops[row.Period].Add(row);
            }

            foreach (KeyValuePair<string, List<Csv.TaifexOP>> entry in ops)
            {
                var cp = new SortedDictionary<double, Csv.OP>();
                foreach (Csv.TaifexOP op in entry.Value)
                {
                    Csv.OP v;
                    int total = Convert.ToInt32(op.Total);

                    if (!cp.ContainsKey(op.Price))
                    {
                        v = new Csv.OP() { Price = (int)op.Price };
                        cp[op.Price] = v;
                    }
                    else
                    {
                        v = cp[op.Price];
                    }

                    if (op.CP == "C")
                    {
                        v.C = total;
                    }
                    else
                    {
                        v.P = total;
                    }
                }

                var list = new List<Csv.OP>();
                foreach (var item in cp.Values)
                {
                    list.Add(item);
                }

                string dir = sourceDir + "\\" + entry.Key;
                string date = entry.Value[0].Date.ToString("yyyy-MM-dd");
                string file = dir + "\\" + date + ".csv";

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(file))
                {
                    FileStream f = File.Create(file);
                    f.Close();
                    f.Dispose();
                }

                using var writer = new StreamWriter(file, false, Encoding.UTF8);
                using var csvw = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csvw.WriteRecords(list);
            }

            return true;
        }
    }
}
