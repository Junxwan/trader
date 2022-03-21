using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// List.xaml 的互動邏輯
    /// </summary>
    public partial class List : UserControl
    {
        public event PropertyChangedEventHandler? PropertyChanged;

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
        DependencyProperty.Register("OPDSource", typeof(List<OPDView>), typeof(List));

        public string OPListColor
        {
            get
            {
                return (string)GetValue(OPListColorProperty);
            }
            set
            {
                SetValue(OPListColorProperty, value);
            }
        }

        public static readonly DependencyProperty OPListColorProperty =
        DependencyProperty.Register("OPListColor", typeof(string), typeof(List));

        public string OMargin
        {
            get
            {
                return (string)GetValue(OMarginProperty);
            }
            set
            {
                SetValue(OMarginProperty, value);
            }
        }

        public static readonly DependencyProperty OMarginProperty =
        DependencyProperty.Register("OMargin", typeof(string), typeof(List));

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public List()
        {
            InitializeComponent();
        }
    }
}
