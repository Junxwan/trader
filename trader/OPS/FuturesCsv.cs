using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.OPS
{
    public class FuturesCsv
    {
        [Name("Date")]
        public DateTime DateTime { get; set; }

        [Name("Open")]
        public int Open { get; set; }

        [Name("Close")]
        public int Close { get; set; }

        [Name("High")]
        public int High { get; set; }

        [Name("Low")]
        public int Low { get; set; }
    }
}
