using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LiveCharts;
using LiveCharts.Wpf;

namespace DatabaseQuery.Views
{
    public partial class PieChartView : UserControl
    {
        public PieChartView()
        {
            InitializeComponent();
            
            PointLabel = chartPoint =>
                $"{chartPoint.Y} ({chartPoint.Participation:P})";
        }

        #region PointLabel

        public static readonly DependencyProperty PointLabelProperty = DependencyProperty.Register(
            "PointLabel", typeof(Func<ChartPoint,string>), typeof(PieChartView), new PropertyMetadata(default(Func<ChartPoint,string>)));

        public Func<ChartPoint,string> PointLabel
        {
            get { return (Func<ChartPoint,string>) GetValue(PointLabelProperty); }
            set { SetValue(PointLabelProperty, value); }
        }


        /// <summary>
        /// Push out the selected pie part
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="chartpoint"></param>
        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (PieChart) chartpoint.ChartView;
            
            //clear selected slice.
            foreach (var seriesView in chart.Series)
            {
                var series = (PieSeries) seriesView;
                series.PushOut = 0;
            }

            var selectedSeries = (PieSeries) chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }

        #endregion

        #region PieChartData

        public static readonly DependencyProperty PieChartDataProperty = DependencyProperty.Register(
            "PieChartData", typeof(Dictionary<string,int>), typeof(PieChartView), new PropertyMetadata(default(Dictionary<string,int>), OnPieChartDataChanged));

        private static void OnPieChartDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (PieChartView) d;
            var newData = (Dictionary<string, int>) e.NewValue;
            if(newData == null) return;

            view.GenPieChart(newData);
        }

        private void GenPieChart(Dictionary<string,int> pieChartData)
        {
            PART_PieChartControl.Series.Clear();
            var sum = pieChartData.Values.Sum();
            
            var pointLabelBinding = new Binding(nameof(PointLabel)){Source = this};
            foreach (var key in pieChartData.Keys)
            {
                // Calculate proportion
                var proportion = pieChartData[key] / (double) sum;
                var proportionText = $"{proportion * 100 :F1}%";
                
                var series = new PieSeries() {Title = key, Values = new ChartValues<int>(new[] {pieChartData[key]}), ToolTip = proportionText};
                series.SetBinding(PointLabelProperty, pointLabelBinding);
                PART_PieChartControl.Series.Add(series);
            }
        }

        public Dictionary<string,int> PieChartData
        {
            get { return (Dictionary<string,int>) GetValue(PieChartDataProperty); }
            set { SetValue(PieChartDataProperty, value); }
        }

        #endregion
    }
}