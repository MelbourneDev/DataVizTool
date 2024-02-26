using DataViz.Interfaces;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataViz.Classes
{
    public class BarChartVisualization : IChartVisualization
    {
        public void GenerateVisualization(PlotModel model, Dictionary<string, List<string>> data, string categoricalField, string numericalField)
        {
            var categoryAxis = new CategoryAxis { Position = AxisPosition.Left, Title = categoricalField };
            var valueAxis = new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, AbsoluteMinimum = 0, Title = numericalField };
            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            var series = new BarSeries();
            foreach (var category in data[categoricalField].Distinct())
            {
                var index = data[categoricalField].IndexOf(category);
                if (index != -1 && index < data[numericalField].Count)
                {
                    if (double.TryParse(data[numericalField][index], out double value))
                    {
                        series.Items.Add(new BarItem { Value = value });
                        categoryAxis.Labels.Add(category);
                    }
                }
            }
            model.Series.Add(series);
        }
    }

}
