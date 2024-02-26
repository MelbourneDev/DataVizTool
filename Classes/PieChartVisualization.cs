using DataViz.Interfaces;
using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataViz.Classes
{
    public class PieChartVisualization : IChartVisualization
    {
        public void GenerateVisualization(PlotModel model, Dictionary<string, List<string>> data, string categoricalField, string numericalField)
        {
            var pieSeries = new PieSeries();

            foreach (var category in data[categoricalField].Distinct())
            {
                var indexes = data[categoricalField]
                    .Select((value, index) => new { value, index })
                    .Where(pair => pair.value == category)
                    .Select(pair => pair.index);

                foreach (var index in indexes)
                {
                    if (index < data[numericalField].Count && double.TryParse(data[numericalField][index], out double value))
                    {
                        pieSeries.Slices.Add(new PieSlice(category, value));
                    }
                }
            }

            model.Series.Add(pieSeries);
        }
    }
}
