using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader.OPS.Csv
{
    public class Value
    {
        //時間
        [Format("yyyy-MM-dd HH:mm:ss")]
        [Index(0)]
        public DateTime Time { get; set; }

        //期貨指數
        [Index(1)]
        public Double Futures { get; set; }

        //Call權利金
        [Index(2)]
        public Double Call { get; set; }

        //Put權利金
        [Index(3)]
        public Double Put { get; set; }

        //總和權利金
        [Index(4)]
        public Double Total { get { return Call + Put; } set { } }

        //履約價
        [Index(5)]
        public int Price { get; set; }

        //週別
        [Index(6)]
        public String Period { get; set; }

        //call可履約
        [Index(7)]
        public bool Call_IS_Fulfillment { get { return Futures > Price; } set { } }

        //put可履約
        [Index(8)]
        public bool Put_IS_Fulfillment { get { return Futures < Price; } set { } }

        //call價內價值
        [Index(9)]
        public Double Call_In_Price_Value { get { return this.Call_IS_Fulfillment ? Futures - Price : 0; } set { } }

        //call時間價值
        [Index(10)]
        public Double Call_Time_Price_Value { get { return Call - this.Call_In_Price_Value; } set { } }

        //put價內價值
        [Index(11)]
        public Double Put_In_Price_Value { get { return this.Put_IS_Fulfillment ? Price - Futures : 0; } set { } }

        //put時間價值
        [Index(12)]
        public Double Put_Time_Price_Value { get { return Put - this.Put_In_Price_Value; } set { } }
    }
}
