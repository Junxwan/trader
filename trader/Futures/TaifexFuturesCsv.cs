using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace trader.Futures
{
    public class TaifexFuturesCsv
    {
        //交易日期
        [Index(0)]
        public DateTime Date { get; set; }

        // 契約
        [Index(1)]
        public string Type { get; set; } = "";

        //到期月份(週別)
        private string period = "";

        [Index(2)]
        public string Period
        {
            get => period;
            set { period = value.Trim(); }
        }

        //開盤價
        [Index(3)]
        public string Open { get; set; } = "0";

        //最高價
        [Index(4)]
        public string High { get; set; } = "0";

        //最低價
        [Index(5)]
        public string Low { get; set; } = "0";

        //收盤價
        [Index(6)]
        public string Close { get; set; } = "0";

        //漲跌
        [Index(7)]
        public string Change { get; set; } = "0";

        //成交量
        [Index(9)]
        public int Volume { get; set; } = 0;

        //結算價
        [Index(10)]
        public string Settlement { get; set; } = "";

        //交易時段
        [Index(17)]
        public string S { get; set; } = "盤後";

        //資料正確性
        public bool IsUse()
        {
            return this.Type == "TX" && this.S == "一般" && this.Volume > 0;
        }
    }
}
