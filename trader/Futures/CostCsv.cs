using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace trader.Futures
{
    public class CostCsv
    {
        //交易日期
        [Format("yyyy/M/d")]
        [Name("Date")]
        public DateTime Date { get; set; }

        //到期月份(週別)
        [Name("Period")]
        public string Period { get; set; } = "";

        //開始時間
        [Format("yyyy/M/d HH:mm:ss")]
        [Name("StartDateTime")]
        public DateTime StartDateTime { get; set; }

        //結束時間
        [Format("yyyy/M/d HH:mm:ss")]
        [Name("EndDateTime")]
        public DateTime EndDateTime { get; set; }

        //總口數
        [Name("Volume")]
        public int Volume { get; set; } = 0;

        //總金額
        [Name("Money")]
        public Int64 Money { get; set; } = 0;

        //平均金額
        [Name("AvgCost")]
        public double AvgCost
        {
            get { return Money > 0 ? Money / Volume : 0; }
            set { }
        }
    }
}
