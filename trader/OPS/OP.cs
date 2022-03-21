using CsvHelper;
using CsvHelper.Configuration.Attributes;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace trader.OPS
{
    //某個履約價的OP未平倉
    public class OP
    {
        public enum Type
        {
            //賣權
            PUT,
            //買權
            CALL,
        }

        //跟上次比變化值
        public int Change { get { return this.change; } }
        private int change;

        //未平倉
        public int Total { get { return this.total; } }
        private readonly int total;

        //履約價
        public int PerformancePrice { get { return this.performancePrice; } }
        private readonly int performancePrice;

        //買賣權
        public Type CP { get; private set; }

        //是否履約
        private bool isPerformance = false;

        //是否為當天未平倉變化減少最多
        public bool IsMaxSubChangeForDay = false;

        //是否為當天未平倉變化增加最多
        public bool IsMaxAddChangeForDay = false;

        public OP(int total, int performancePrice, Type cp)
        {
            this.CP = cp;
            this.total = total;
            this.performancePrice = performancePrice;
        }

        //指數價格
        public void SetPrice(int price)
        {
            if (this.CP == Type.CALL)
            {
                this.isPerformance = (price >= this.performancePrice);
            }
            else
            {
                this.isPerformance = (price <= this.performancePrice);
            }
        }

        //未平倉變化
        public void SetChange(OP op)
        {
            this.change = this.total - op.total;
        }

        //是否履約
        public bool IsPerformance()
        {
            return this.isPerformance;
        }
    }
}
