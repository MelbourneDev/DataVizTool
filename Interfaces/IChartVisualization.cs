using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataViz.Interfaces
{
    public interface IChartVisualization
    {

        void GenerateVisualization(PlotModel model, Dictionary<string, List<string>> data, string categoricalField, string numericalField);

    }
}
