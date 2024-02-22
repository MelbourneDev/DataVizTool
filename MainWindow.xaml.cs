using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;


namespace DataViz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();

        public MainWindow()
        {
            InitializeComponent();
          
            lstNumerical.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            lstCategorical.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
        }
        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox != null && listBox.SelectedItem != null)
            {
                string dataToDrag = listBox.SelectedItem.ToString();
                DragDrop.DoDragDrop(listBox, dataToDrag, DragDropEffects.Move);
            }
        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            Debug.WriteLine("DragOver event triggered");
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void btnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                data = ParseCsv(filename);
                var (numerical, categorical) = ClassifyFields(data);
                lstNumerical.ItemsSource = numerical;
                lstCategorical.ItemsSource = categorical;
            }
        }


        private Dictionary<string, List<string>> ParseCsv(string filePath)
        {
            var data = new Dictionary<string, List<string>>();
            var lines = File.ReadAllLines(filePath);

            if (lines.Length > 0)
            {
                // Split the header line into column names
                var headers = lines[0].Split(',');

                foreach (var header in headers)
                {
                    data[header] = new List<string>();
                }

                // Parse each line after the header
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');

                    // Ensure the values array is not shorter than the headers array
                    if (values.Length >= headers.Length)
                    {
                        for (int j = 0; j < headers.Length; j++)
                        {
                            // Safely add value or an empty string if the value is missing
                            data[headers[j]].Add(values.Length > j ? values[j] : "");
                        }
                    }
                    else
                    {
                        // Handle or log the case where a line has fewer values than expected
                        Console.WriteLine($"Line {i + 1} has fewer values than headers. Line skipped.");
                    }
                }
            }

            return data;
        }


        private (List<string> numerical, List<string> categorical) ClassifyFields(Dictionary<string, List<string>> data)
        {
            var numerical = new List<string>();
            var categorical = new List<string>();

            foreach (var entry in data)
            {
                // Assuming the first row (after header) is representative for type checking
                bool isNumeric = double.TryParse(entry.Value.FirstOrDefault(), out _);
                if (isNumeric)
                {
                    numerical.Add(entry.Key);
                }
                else
                {
                    categorical.Add(entry.Key);
                }
            }

            return (numerical, categorical);
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            Debug.WriteLine("DragOver event triggered");
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                {
                    data = ParseCsv(files[0]);
                    var (numerical, categorical) = ClassifyFields(data);
                    lstNumerical.ItemsSource = numerical;
                    lstCategorical.ItemsSource = categorical;
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string fieldName = (string)e.Data.GetData(DataFormats.StringFormat);
                // For simplicity, directly calling GenerateVisualization for any field dropped
                // Assuming a default category for demonstration
                GenerateVisualization(fieldName, "YourCategoricalField"); // Replace "YourCategoricalField" as needed
            }
        }


        private void GenerateBarChart(PlotModel model, List<string> fields)
        {
            // Assuming the first field is categorical and the second is numerical
            var categoryAxis = new CategoryAxis { Position = AxisPosition.Left }; // For vertical bars, use Left or Right
            var valueAxis = new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, AbsoluteMinimum = 0 };

            // Configure axes
            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            var series = new BarSeries(); // Use BarSeries instead of ColumnSeries

            // Example logic to populate series based on fields
            // This example assumes 'fields' contains at least one categorical and one numerical field
            if (fields.Count >= 2 && data.ContainsKey(fields[0]) && data.ContainsKey(fields[1]))
            {
                string categoricalField = fields[0];
                string numericalField = fields[1];

                foreach (var category in data[categoricalField].Distinct())
                {
                    var index = data[categoricalField].IndexOf(category);
                    if (index != -1 && index < data[numericalField].Count)
                    {
                        if (double.TryParse(data[numericalField][index], out double value))
                        {
                            // For BarSeries, you add BarItems
                            series.Items.Add(new BarItem { Value = value });
                            // In a vertical bar chart, categories go on the Y-axis
                            categoryAxis.Labels.Add(category);
                        }
                    }
                }
            }

            model.Series.Add(series);
        }


        private void GenerateVisualization(string categoricalField, string numericalField)
        {
            if (!data.ContainsKey(numericalField) || !data.ContainsKey(categoricalField)) return;

            var model = new PlotModel { Title = "Generated Visualization" };

            // Ensuring that a CategoryAxis is on the Y-axis
            var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
            categoryAxis.Title = categoricalField;

            // Ensuring that a LinearAxis (or other suitable numerical axis) is on the X-axis
            var valueAxis = new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, AbsoluteMinimum = 0 };
            valueAxis.Title = numericalField;

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
            plotView.Model = model;
        }


        // Field selections stored to keep track of user choices
        private string selectedNumericalField = null;
        private string selectedCategoricalField = null;

        private void VisualizationArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string fieldName = (string)e.Data.GetData(DataFormats.StringFormat);
                // Determine field type and store selection
                if (lstNumerical.Items.Contains(fieldName))
                {
                    selectedNumericalField = fieldName;
                }
                else if (lstCategorical.Items.Contains(fieldName))
                {
                    selectedCategoricalField = fieldName;
                }

                // If both fields have been selected, generate the visualization
                if (selectedNumericalField != null && selectedCategoricalField != null)
                {
                    GenerateVisualization(selectedCategoricalField, selectedNumericalField);

                    // Reset selections for next visualization
                    selectedNumericalField = null;
                    selectedCategoricalField = null;
                }
            }
        }




    }

}