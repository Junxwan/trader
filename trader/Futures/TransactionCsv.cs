using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.Futures
{
    public class TransactionCsv
    {
        //交易日期
        [Index(0)]
        public DateTime Date { get; set; }

        // 契約
        [Index(1)]
        public string Type { get; set; }

        //到期月份(週別)
        [Index(2)]
        public string Period { get; set; }

        //成交時間
        [Index(3)]
        public int Time { get; set; }

        //成交價
        [Index(4)]
        public double Price { get; set; }

        //成交量
        [Index(5)]
        public int Volume { get; set; }

        //完整交易時間
        public DateTime DateTime { get; set; }
    }
}
