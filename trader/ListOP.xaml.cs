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

namespace trader
{
    /// <summary>
    /// ListOP.xaml 的互動邏輯
    /// </summary>
    public partial class ListOP : UserControl
    {
        public OPW OPWSource
        {
            get
            {
                return (OPW)GetValue(OPWSourceProperty);
            }
            set
            {
                SetValue(OPWSourceProperty, value);
            }
        }

        public static readonly DependencyProperty OPWSourceProperty =
        DependencyProperty.Register("OPWSource", typeof(OPW), typeof(ListOP));

        public OP.Type CP
        {
            get
            {
                return (OP.Type)GetValue(OPTypeProperty);
            }
            set
            {
                SetValue(OPTypeProperty, value);
            }
        }

        public static readonly DependencyProperty OPTypeProperty =
        DependencyProperty.Register("OPType", typeof(OP.Type), typeof(ListOP));

        public List<OPView> Value { get; set; }

        public ListOP()
        {
            InitializeComponent();
            this.Value = new List<OPView>();

            foreach (var item in OPWSource.Value)
            {
                this.Value.Add(new OPView(item, CP));
            }
        }
    }

    public class OPView
    {
        public string DateText { get; private set; }

        public string PriceText { get; private set; }

        public List<OP> Value { get; private set; }

        public OPView(OPD o, OP.Type cp)
        {
            this.DateText = o.DateText;
            this.PriceText = o.PriceText;

            if (cp == OP.Type.PUT)
            {
                this.Value = o.Puts;
            }
            else
            {
                this.Value = o.Calls;
            }
        }
    }
}
