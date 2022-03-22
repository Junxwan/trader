using CsvHelper;
using CsvHelper.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.Futures
{
    public class Price
    {
        private string fileName = "prices";

        private string sourceDir;

        private string filePath = "";

        private SortedList<DateTime, FuturesCsv> Data;

        public Price(string sourceDir)
        {
            this.sourceDir = sourceDir;
            this.filePath = this.sourceDir + "\\" + this.fileName + ".csv";
            this.Data = new SortedList<DateTime, FuturesCsv>();
        }

        public void Load()
        {
            if (this.Data.Count > 0)
            {
                return;
            }

            if (File.Exists(this.filePath))
            {
                using var reader = new StreamReader(this.filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                foreach (var item in csv.GetRecords<FuturesCsv>())
                {
                    this.Data.Add(item.Date, item);
                }
            }
        }

        //下載台指收盤資料
        public bool Download(DateTime datetime)
        {
            this.Load();

            if (this.Data.ContainsKey(datetime))
            {
                return true;
            }

            var client = new RestClient("https://www.taifex.com.tw");
            var request = new RestRequest("/cht/3/futDataDown", Method.POST);
            string date = datetime.ToString("yyyy/MM/dd");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("down_type", "1");
            request.AddParameter("commodity_id", "TX");
            request.AddParameter("commodity_id2", "");
            request.AddParameter("queryStartDate", date);
            request.AddParameter("queryEndDate", date);

            RestResponse resp = (RestResponse)client.Execute(request);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var result = (Encoding.GetEncoding(950)).GetString(resp.RawBytes);
            using var csv = new CsvReader(new StringReader(result), CultureInfo.InvariantCulture);
            return TaifexFuturesToCsv(csv.GetRecords<TaifexFuturesCsv>(), this.sourceDir);
        }

        //下載台指結算價
        public TaifexSettlemenFuturesCsv? DownloadSettlement(DateTime start, DateTime end)
        {
            var client = new RestClient("https://www.taifex.com.tw");
            var request = new RestRequest("/cht/5/fSPDataDown", Method.POST);
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("start_year", start.Year);
            request.AddParameter("start_month", start.ToString("MM"));
            request.AddParameter("end_year", end.Year);
            request.AddParameter("end_month", end.ToString("MM"));
            request.AddParameter("dlFileType", 1);

            RestResponse resp = (RestResponse)client.Execute(request);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var result = (Encoding.GetEncoding(950)).GetString(resp.RawBytes);
            using var csv = new CsvReader(new StringReader(result), CultureInfo.InvariantCulture);

            foreach (var item in csv.GetRecords<TaifexSettlemenFuturesCsv>())
            {
                if (item.Date == end && item.Code == "TX/MTX")
                {
                    return item;
                }
            }

            return null;
        }

        //整理台指收盤資料
        public bool TaifexFuturesToCsv(IEnumerable<TaifexFuturesCsv> csv, string sourceDir)
        {
            FuturesCsv data = new FuturesCsv();

            foreach (var row in csv)
            {
                if (!row.IsUse())
                {
                    continue;
                }

                data = new FuturesCsv() { Date = row.Date, Period = row.Period };
                data.Open = Convert.ToInt32(row.Open);
                data.Close = Convert.ToInt32(row.Close);
                data.High = Convert.ToInt32(row.High);
                data.Low = Convert.ToInt32(row.Low);
                data.Settlement = Convert.ToInt32(row.Settlement);
                data.Change = Convert.ToInt32(row.Change);

                //通常第一筆就是當日行情
                break;
            }

            if (data.Settlement == 0)
            {
                var settlement = this.DownloadSettlement(data.Date.AddMonths(-1), data.Date);

                if (settlement != null)
                {
                    data.Settlement = (int)settlement.Price;
                }
            }

            CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture);

            if (!File.Exists(this.filePath))
            {
                FileStream f = File.Create(this.filePath);
                f.Close();
                f.Dispose();

                csvConfig.HasHeaderRecord = true;
            }
            else
            {
                csvConfig.HasHeaderRecord = false;
            }

            using var writer = new StreamWriter(this.filePath, true, Encoding.UTF8);
            using var csvw = new CsvWriter(writer, csvConfig);
            csvw.WriteRecords(new List<FuturesCsv>() { data });

            return true;
        }

        //全部資料
        public SortedList<DateTime, FuturesCsv> All()
        {
            this.Load();
            return this.Data;
        }
    }
}
