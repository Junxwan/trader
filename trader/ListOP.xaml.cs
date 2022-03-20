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
}
