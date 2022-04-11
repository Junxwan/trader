using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.OPS.Csv
{
    public class Transaction
    {
        //交易日期
        [Index(0)]
        public DateTime Date { get; set; }

        // 契約
        [Index(1)]
        public string Type { get; set; }

        //履約價
        [Index(2)]
        public int Performance { get; set; }

        //到期月份(週別)
        [Index(3)]
        public string Period { get; set; }

        //買賣權
        [Index(4)]
        public string Cp { get; set; }

        //成交時間
        [Index(5)]
        public int Time { get; set; }

        //成交價
        [Index(6)]
        public double Price { get; set; }

        //成交量
        [Index(7)]
        public int Volume { get; set; }

        //完整交易時間
        public DateTime DateTime { get; set; }
    }
}
