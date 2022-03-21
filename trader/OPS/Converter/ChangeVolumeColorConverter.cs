using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace trader.OPS.Converter
{
    //未平倉增減
    public class ChangeVolumeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var op = (OP)value;
            return (string)parameter switch
            {
                "IsMaxSubChangeForDay" => op.IsMaxSubChangeForDay,
                "IsMaxAddChangeForDay" => op.IsMaxAddChangeForDay,
                _ => false,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
