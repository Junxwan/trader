using ScottPlot;
using ScottPlot.Plottable;
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

namespace trader.OPS.Chart
{
    /// <summary>
    /// CallPutLine.xaml 的互動邏輯
    /// </summary>
    public partial class CallPutLine : UserControl
    {
        private WpfPlot line;

        private Crosshair Crosshair;

        public CallPutLine()
        {
            InitializeComponent();
            this.line = this.chart;
            Crosshair = this.line.Plot.AddCrosshair(0, 0);
            Crosshair.IsVisible = false;
        }

        public void DrawTotal(List<OP> call, List<OP> put, int[] prices, int price)
        {
            var callTotal = new List<double>();
            var putTotal = new List<double>();

            foreach (var item in call)
            {
                var total = 0.0;

                if (callTotal.Count > 0)
                {
                    total = callTotal.Last() + item.Total;
                }
                else
                {
                    total = item.Total;
                }

                callTotal.Add(total);
            }

            foreach (var item in put)
            {
                var total = 0.0;

                if (putTotal.Count > 0)
                {
                    total = putTotal.Last() + item.Total;
                }
                else
                {
                    total = item.Total;
                }

                putTotal.Add(total);
            }

            putTotal.Reverse();

            this.line.Plot.Clear();

            var index = 0;
            foreach (var item in prices)
            {
                if (item >= price)
                {
                    this.line.Plot.AddVerticalLine(index, color: System.Drawing.Color.White);
                    break;
                }

                index++;
            }

            double[] positions = DataGen.Consecutive(prices.Length);
            this.line.Plot.AddScatter(positions, callTotal.ToArray(), label: "Call", color: System.Drawing.Color.Red);
            this.line.Plot.AddScatter(positions, putTotal.ToArray(), label: "Put", color: System.Drawing.Color.Green);

            string[] labels = prices.Select(x => x.ToString()).ToArray();
            this.line.Plot.XAxis.ManualTickPositions(positions, labels);
            this.line.Plot.Title("Call/Put累積未平倉");
            this.line.Plot.XLabel("履約價");
            this.line.Plot.Title("累積未平倉");
            this.line.Plot.Legend();
            this.line.Plot.Style(ScottPlot.Style.Gray2);
            this.line.Plot.XAxis.Color(System.Drawing.Color.White);
            this.line.Plot.YAxis.Color(System.Drawing.Color.White);
            Crosshair = this.line.Plot.AddCrosshair(0, 0);
            Crosshair.Color = System.Drawing.Color.CadetBlue;
            this.line.Refresh();
        }

        public void DrawChange(List<OP> call, List<OP> put, int[] prices, int price)
        {
            var callTotal = new List<double>();
            var putTotal = new List<double>();

            foreach (var item in call)
            {
                callTotal.Add(item.Change);
            }

            foreach (var item in put)
            {
                putTotal.Add(item.Change);
            }

            putTotal.Reverse();

            this.line.Plot.Clear();

            var index = 0;
            foreach (var item in prices)
            {
                if (item >= price)
                {
                    this.line.Plot.AddVerticalLine(index, color: System.Drawing.Color.White);
                    break;
                }

                index++;
            }

            double[] positions = DataGen.Consecutive(prices.Length);
            this.line.Plot.AddScatter(positions, callTotal.ToArray(), label: "Call", color: System.Drawing.Color.Red);
            this.line.Plot.AddScatter(positions, putTotal.ToArray(), label: "Put", color: System.Drawing.Color.Green);

            string[] labels = prices.Select(x => x.ToString()).ToArray();
            this.line.Plot.XAxis.ManualTickPositions(positions, labels);
            this.line.Plot.AddHorizontalLine(0, color: System.Drawing.Color.White);
            this.line.Plot.Title("Call/Put未平倉增減");
            this.line.Plot.XLabel("履約價");
            this.line.Plot.Title("未平倉增減");
            this.line.Plot.Legend();
            this.line.Plot.Style(ScottPlot.Style.Gray2);
            this.line.Plot.XAxis.Color(System.Drawing.Color.White);
            this.line.Plot.YAxis.Color(System.Drawing.Color.White);
            Crosshair = this.line.Plot.AddCrosshair(0, 0);
            Crosshair.Color = System.Drawing.Color.CadetBlue;
            this.line.Refresh();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            int pixelX = (int)e.MouseDevice.GetPosition(this.line).X;
            int pixelY = (int)e.MouseDevice.GetPosition(this.line).Y;

            (double coordinateX, double coordinateY) = this.line.GetMouseCoordinates();

            Crosshair.X = coordinateX;
            Crosshair.Y = coordinateY;

            this.line.Refresh();
        }

        private void wpfPlot1_MouseEnter(object sender, MouseEventArgs e)
        {
            Crosshair.IsVisible = true;
        }

        private void wpfPlot1_MouseLeave(object sender, MouseEventArgs e)
        {
            Crosshair.IsVisible = false;
            this.line.Refresh();
        }
    }
}
