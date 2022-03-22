using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace trader.OPS
{
    public class Style : Freezable
    {
        public int SOPChangeColumnIndex
        {
            get
            {
                return (int)GetValue(SOPChangeColumnIndexProperty);
            }
            set
            {
                SetValue(SOPChangeColumnIndexProperty, value);
            }
        }

        public static readonly DependencyProperty SOPChangeColumnIndexProperty =
            DependencyProperty.Register("SOPChangeColumnIndex", typeof(int), typeof(Style), new PropertyMetadata(0));

        public int SOPTotalColumnIndex
        {
            get
            {
                return (int)GetValue(SOPTotalColumnIndexProperty);
            }
            set
            {
                SetValue(SOPTotalColumnIndexProperty, value);
            }
        }

        public static readonly DependencyProperty SOPTotalColumnIndexProperty =
            DependencyProperty.Register("SOPTotalColumnIndex", typeof(int), typeof(Style), new PropertyMetadata(1));

        public int SOPIsPerformanceColumnIndex
        {
            get
            {
                return (int)GetValue(SOPIsPerformanceColumnIndexProperty);
            }
            set
            {
                SetValue(SOPIsPerformanceColumnIndexProperty, value);
            }
        }

        public static readonly DependencyProperty SOPIsPerformanceColumnIndexProperty =
            DependencyProperty.Register("SOPIsPerformanceColumnIndex", typeof(int), typeof(Style), new PropertyMetadata(2));

        protected override Freezable CreateInstanceCore()
        {
            return new Style();
        }
    }
}
