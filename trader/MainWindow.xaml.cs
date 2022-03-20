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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<OPD> List { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            var mag = new OPManage("G:\\我的雲端硬碟\\金融\\data\\op\\");
            var opw = mag.Get("202203");
            this.List = opw.Value;

            this.DataContext = this;
        }
    }

    public class OTextBlock : TextBlock
    {
        public static readonly DependencyProperty UserSourceProperty;

        public OP User
        {
            get
            {
                return (OP)GetValue(UserSourceProperty);
            }
            set
            {
                SetValue(UserSourceProperty, value);
            }
        }

        static OTextBlock()
        {
            UserSourceProperty = DependencyProperty.Register("UserSourceProperty", typeof(OP), typeof(OTextBlock), new FrameworkPropertyMetadata(OnUserSource));
        }

        private static void OnUserSource(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }

    public class XxxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGridCell cell = (DataGridCell)value;
            OP user = (OP)cell.DataContext;
            int v = 0;
            string p = (string)parameter;

            if (cell.Column.DisplayIndex == 0)
            {
                v = user.Change;
            }
            else if (cell.Column.DisplayIndex == 1)
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

    public class IsPerformanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((OP)value).IsPerformance();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
