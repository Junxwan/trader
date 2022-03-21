using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace trader.OPS
{
    /// <summary>
    /// Table.xaml 的互動邏輯
    /// </summary>
    public partial class Table : UserControl
    {
        public List<OP> OPSource
        {
            get
            {
                return (List<OP>)GetValue(OPSourceProperty);
            }
            set
            {
                SetValue(OPSourceProperty, value);
            }
        }

        public static readonly DependencyProperty OPSourceProperty =
        DependencyProperty.Register("OPSource", typeof(List<OP>), typeof(Table), new PropertyMetadata());

        public string DateText
        {
            get
            {
                return (string)GetValue(DateTextProperty);
            }
            set
            {
                SetValue(DateTextProperty, value);
            }
        }

        public static readonly DependencyProperty DateTextProperty =
         DependencyProperty.Register("DateText", typeof(string), typeof(Table), new PropertyMetadata("0"));

        public string PriceText
        {
            get
            {
                return (string)GetValue(PriceTextProperty);
            }
            set
            {
                SetValue(PriceTextProperty, value);
            }
        }

        public static readonly DependencyProperty PriceTextProperty =
            DependencyProperty.Register("PriceText", typeof(string), typeof(Table), new PropertyMetadata("0"));

        public string OPColor
        {
            get
            {
                return (string)GetValue(OPColorProperty);
            }
            set
            {
                SetValue(OPColorProperty, value);
            }
        }

        public static readonly DependencyProperty OPColorProperty =
        DependencyProperty.Register("OPColor", typeof(string), typeof(Table), new PropertyMetadata("Black"));

        public Table()
        {
            InitializeComponent();
        }
    }
}
