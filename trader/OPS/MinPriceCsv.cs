using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace trader.OPS
{
    public class MinPriceCsv
    {
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
