using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace trader
{
    /// <summary>
    /// ListOP.xaml 的互動邏輯
    /// </summary>
    public partial class ListOP : UserControl
    {
        public List<OPDView> OPDSource
        {
            get
            {
                return (List<OPDView>)GetValue(OPDSourceProperty);
            }
            set
            {
                SetValue(OPDSourceProperty, value);
            }
        }

        public static readonly DependencyProperty OPDSourceProperty =
        DependencyProperty.Register("OPDSource", typeof(List<OPDView>), typeof(ListOP));     

        public ListOP()
        {
            InitializeComponent();
        }
    }

    public class OPDView
    {
        private readonly OPD opd;

        private readonly OP.Type type;

        public string DateText
        {
            get
            {
                return this.opd.Date() + "(" + this.opd.Week() + ")";
            }
            private set { }
        }

        public string PriceText { get; set; } = "0";

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
