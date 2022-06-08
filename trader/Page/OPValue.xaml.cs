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
using System.Windows.Shapes;
using trader.Futures;
using trader.OPS;

namespace trader.Page
{
    /// <summary>
    /// OPValue.xaml 的互動邏輯
    /// </summary>
    public partial class OPValue : Window
    {
        public Manage OP { get; set; }

        public OPS.Transaction Transaction { get; set; }

        public OPValue()
        {
            var dataPath = (new Config()).GetData("Path");
            OP = new Manage(dataPath, new Price(dataPath));
            Transaction = new OPS.Transaction(dataPath);

            InitializeComponent();

            opPeriodsComboBox.ItemsSource = this.OP.Periods;
            opPeriodsComboBox.SelectedIndex = 0;
        }

        private void opPeriodsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (opPeriodsComboBox.SelectedValue == null)
            {
                return;
            }

            if (datePicker.SelectedDate == null)
            {
                return;
            }

            var date = DateTime.Parse(datePicker.SelectedDate.ToString());
            var data = Transaction.Get5MinKRange(opPeriodsComboBox.SelectedValue.ToString(), date, date.AddDays(7));

            foreach (var period in data.Keys)
            {

            }
            
            foreach (KeyValuePair<string, Dictionary<string, List<OPS.Csv.MinPrice>>> row in data)
            {

            }
        }
    }
}
