using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.Models;
using LiveCharts;
using LiveCharts.Wpf;

namespace DatabaseQuery.Views
{
    public partial class LineChartView : UserControl
    {
        public LineChartView()
        {
            InitializeComponent();
        }

        #region SeriesDataProperty

        public static readonly DependencyProperty ProductivityDataProperty = DependencyProperty.Register(
            "ProductivityData", typeof(LineChartData), typeof(LineChartView),
            new PropertyMetadata(default(LineChartData)));
        

        public LineChartData ProductivityData
        {
            get { return (LineChartData) GetValue(ProductivityDataProperty); }
            set { SetValue(ProductivityDataProperty, value); }
        }

        #endregion

        #region YieldDataProperty

        public static readonly DependencyProperty YieldDataProperty = DependencyProperty.Register(
            "YieldData", typeof(LineChartData), typeof(LineChartView), new PropertyMetadata(default(LineChartData)));

        public LineChartData YieldData
        {
            get { return (LineChartData) GetValue(YieldDataProperty); }
            set { SetValue(YieldDataProperty, value); }
        }

        #endregion

        private void OnSwitchViewButtonClick(object sender, RoutedEventArgs e)
        {
            var buttonText = (string) PART_SwitchViewButton.Content;
            // Previously showing productivity line, now switch to yield line
            if (buttonText == "Productivity")
            {
                PART_SwitchViewButton.Content = "Yield";
                ShowYield();
            }
            else // Show productivity line
            {
                PART_SwitchViewButton.Content = "Productivity";
                ShowProductivity();
            }
        }

        private void ShowYield()
        {
            PART_XAxis.Labels = YieldData.Data.Keys.ToList();
            PART_CartesianChart.Series = new SeriesCollection
            {
                new LineSeries()
                {
                    Values = new ChartValues<double>(YieldData.Data.Values), LineSmoothness = 0,
                    Title = $"Yield per {YieldData.UnitType.ToString()}"
                }
            };
        }

        private void OnLineChartViewLoaded(object sender, RoutedEventArgs e)
        {
            // Show productivity data when loaded
            ShowProductivity();
        }

        private void ShowProductivity()
        {
            PART_XAxis.Labels = ProductivityData.Data.Keys.ToList();
            PART_CartesianChart.Series = new SeriesCollection
            {
                new LineSeries()
                {
                    Values = new ChartValues<double>(ProductivityData.Data.Values), LineSmoothness = 0,
                    Title = $"Units per {ProductivityData.UnitType.ToString()}"
                }
            };
        }
    }
}