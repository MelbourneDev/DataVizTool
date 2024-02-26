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
using DataViz.Classes;
using DataViz.Interfaces;


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
             
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";           
            Nullable<bool> result = dlg.ShowDialog();            
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
                // split the header line into column names
                var headers = lines[0].Split(',');

                foreach (var header in headers)
                {
                    data[header] = new List<string>();
                }

                // Parse each line after the header
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');

                    //  array is not shorter than the headers array
                    if (values.Length >= headers.Length)
                    {
                        for (int j = 0; j < headers.Length; j++)
                        {
                            // add value or an empty string if the value is missing
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
                GenerateVisualization(fieldName, "YourCategoricalField", currentChartType);
            }
        }



        private string selectedNumericalField = null;
        private string selectedCategoricalField = null;
        private void GenerateVisualization(string categoricalField, string numericalField, string chartType)
        {
            var model = new PlotModel { Title = "Generated Visualization" };


            IChartVisualization chartVisualization = null;

            switch (chartType)
            {
                case "Bar":
                    chartVisualization = new BarChartVisualization();
                    break;
                case "Line":
                    // chartVisualization = new LineChartVisualization();
                    break;
                case "Pie":
                    chartVisualization = new PieChartVisualization();
                    break;
                default:
                    throw new NotImplementedException($"Chart type {chartType} is not supported.");
            }

            Debug.WriteLine($"Generating {chartType} chart...");
            if (chartVisualization != null)
            {
                chartVisualization.GenerateVisualization(model, data, categoricalField, numericalField);
                plotView.Model = model;
            }
            else
            {
                
                Debug.WriteLine("No chart visualization selected or an unrecognized chart type provided.");
            }

            lastSelectedCategoricalField = categoricalField;
            lastSelectedNumericalField = numericalField;
        }


        // Field selections stored to keep track of user choices
       

        private void VisualizationArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string fieldName = (string)e.Data.GetData(DataFormats.StringFormat);

                // Determine if the dropped field is numerical or categorical
                if (lstNumerical.Items.Contains(fieldName))
                {
                    selectedNumericalField = fieldName;
                }
                else if (lstCategorical.Items.Contains(fieldName))
                {
                    selectedCategoricalField = fieldName;
                }

                // Check if both a numerical and a categorical field have been selected
                if (selectedNumericalField != null && selectedCategoricalField != null)
                {
                    // Call GenerateVisualization with the selected fields
                    GenerateVisualization(selectedCategoricalField, selectedNumericalField, currentChartType);

                    // Reset the selected fields for the next visualization
                    selectedNumericalField = null;
                    selectedCategoricalField = null;
                }
            }
        }


        private string currentChartType = "Bar"; // Default chart type
        private string lastSelectedNumericalField = null;
        private string lastSelectedCategoricalField = null;

        private void ChartTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chartTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                currentChartType = selectedItem.Content.ToString();
                Debug.WriteLine($"Selected chart type: {currentChartType}");

                // Trigger visualization update if fields have already been selected
                if (!string.IsNullOrEmpty(lastSelectedNumericalField) && !string.IsNullOrEmpty(lastSelectedCategoricalField))
                {
                    GenerateVisualization(lastSelectedCategoricalField, lastSelectedNumericalField, currentChartType);
                }
            }
        }





    }


}