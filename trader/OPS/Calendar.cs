using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.OPS
{
    public class Calendar
    {
        private List<Csv.Calendar> data;

        private string dir;

        public Calendar(string dir)
        {
            this.dir = dir;
            this.data = new List<Csv.Calendar>();
        }

        public void Load()
        {
            using (var reader = new StreamReader(this.dir + "/calendar.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                data = csv.GetRecords<Csv.Calendar>().ToList();
            }
        }

        public Csv.Calendar Get(string OpPeriod)
        {
            if (data.Count == 0)
            {
                this.Load();
            }

            foreach (var item in this.data)
            {
                if (item.OP == OpPeriod)
                {
                    return item;
                }
            }

            return new Csv.Calendar();
        }

        public string GetFutures(string OpPeriod)
        {
            return this.Get(OpPeriod).FM;       
        }

        public DateTime GetFirstDate(string OpPeriod)
        {
            return this.Get(OpPeriod).Start;
        }

        public DateTime GetEndDate(string OpPeriod)
        {
            return this.Get(OpPeriod).End;
        }
    }
}
