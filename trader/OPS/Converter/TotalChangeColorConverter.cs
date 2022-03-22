using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace trader.OPS.Converter
{
    //未平倉變化
    public class TotalChangeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGridCell cell = (DataGridCell)value;
            OP user = (OP)cell.DataContext;
            int v = 0;
            string p = (string)parameter;
            
            if ((string)cell.Column.Header == "Change")
            {
                v = user.Change;
            }
            else if ((string)cell.Column.Header == "Total")
            {
                v = user.Total;
            }

            switch (p)
            {
                case "sub":
                    return v < 0;
                case "add":
                    return v > 0;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
