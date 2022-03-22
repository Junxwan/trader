using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.OPS
{
    //OP顯示的資料
    public class OPDView
    {
        private readonly OPD opd;

        private readonly OP.Type type;

        public string DateText
        {
            get
            {
                if (this.opd.DateTime.Year <= 1)
                {
                    return "";
                }

                return this.opd.Date() + "(" + this.opd.Week() + ")";
            }
            private set { }
        }

        public string PriceText
        {
            get
            {
                return this.opd.PriceChange + "/" + this.opd.Price;
            }
            private set { }
        }

        public List<OP> Value
        {
            get
            {
                if (this.type == OP.Type.CALL)
                {
                    return this.opd.Calls;
                }

                return this.opd.Puts;
            }
            private set { }
        }

        public OPDView(OPD op, OP.Type t)
        {
            this.opd = op;
            this.type = t;
        }
    }
}
