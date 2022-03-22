using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace trader.Futures
{
    public class TaifexSettlemenFuturesCsv
    {
        //結算日
        [Index(0)]
        public DateTime Date { get; set; }

        //代碼
        [Index(2)]
        public string Code { get; set; }

        //結算價
        [Index(4)]
        public double Price { get; set; }
    }
}
