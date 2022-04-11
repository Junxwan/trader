using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.OPS
{
    public class PageData
    {
        public List<DayView> CALL { get; set; } = new List<DayView>();

        public List<DayView> PUT { get; set; } = new List<DayView>();

        public int[] Prices { set; get; } = new int[] { };

        public PageData() { }

        public PageData(Week opw, int performance = 0, string date = "")
        {
            List<Day> data;
            var _call = new List<DayView>();
            var _put = new List<DayView>();
            var prices = new int[] { };
            (data, prices) = opw.Page(performance, date);

            foreach (var item in data)
            {
                _call.Add(new DayView(item, OP.Type.CALL));
            }

            data.Reverse();

            foreach (var item in data)
            {
                _put.Add(new DayView(item, OP.Type.PUT));
            }

            this.CALL = _call;
            this.PUT = _put;
            this.Prices = prices;
        }
    }
}
