using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.OPS
{
    public class TaifexOPCsv
    {
        //交易日期
        [Index(0)]
        public DateTime Date { get; set; }

        // 契約
        [Index(1)]
        public string Type { get; set; } = "";

        //到期月份(週別)
        private string period = "";

        [Index(2)]
        public string Period
        {
            get => period;
            set { period = value.Trim(); }
        }

        //履約價
        [Index(3)]
        public double Price { get; set; }

        //買賣權
        private string cp = "";

        [Index(4)]
        public string CP
        {
            get { return cp; }
            set
            {
                if (value == "買權")
                {
                    cp = "C";
                }
                else
                {
                    cp = "P";
                }
            }
        }

        //未沖銷契約數
        [Index(11)]
        public string Total { get; set; } = "0";

        //交易時段
        [Index(17)]
        public string S { get; set; } = "盤後";

        //資料正確性
        public bool IsUse()
        {
            return this.Type == "TXO" && this.S == "一般";
        }
    }
}
