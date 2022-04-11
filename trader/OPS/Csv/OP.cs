using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.OPS.Csv
{
    public class OP
    {
        //合約價
        [Index(0)]
        public int Price { get; set; }

        //call 未平倉
        [Index(1)]
        public int C { get; set; }

        //put 未平倉
        [Index(2)]
        public int P { get; set; }
    }
}
