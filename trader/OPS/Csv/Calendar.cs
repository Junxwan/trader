using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace trader.OPS.Csv
{
    public class Calendar
    {
        //期貨月份
        [Index(0)]
        public string FM { get; set; }

        //OP週別
        [Index(1)]
        public string OP { get; set; }

        //開倉日
        [Format("yyyy-MM-dd")]
        [Index(2)]
        public DateTime Start { get; set; }

        //結算日
        [Format("yyyy-MM-dd")]
        [Index(3)]
        public DateTime End { get; set; }
    }
}
