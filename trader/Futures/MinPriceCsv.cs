using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.Futures
{
    public class MinPriceCsv
    {
        [Format("yyyy-MM-dd HH:mm:ss")]
        [Name("DateTime")]
        public DateTime DateTime { get; set; }

        [Name("Open")]
        public double Open { get; set; }

        [Name("High")]
        public double High { get; set; }

        [Name("Low")]
        public double Low { get; set; }

        [Name("Close")]
        public double Close { get; set; }

        [Name("Volume")]
        public int Volume { get; set; }
    }
}
