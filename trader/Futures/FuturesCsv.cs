using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace trader.Futures
{
    public class FuturesCsv
    {
        //交易日期
        [Format("yyyy/M/d")]
        [Name("Date")]
        public DateTime Date { get; set; }

        //到期月份(週別)
        [Name("Period")]
        public string Period { get; set; } = "";

        //開盤價
        [Name("Open")]
        public int Open { get; set; } = 0;

        //最高價
        [Name("High")]
        public int High { get; set; } = 0;

        //最低價
        [Name("Low")]
        public int Low { get; set; } = 0;

        //收盤價
        [Name("Close")]
        public int Close { get; set; } = 0;

        //結算價
        [Name("Settlement")]
        public int Settlement { get; set; } = 0;

        //漲跌
        [Name("Change")]
        public int Change { get; set; } = 0;
    }
}
