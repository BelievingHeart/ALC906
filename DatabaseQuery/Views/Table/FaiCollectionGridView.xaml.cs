﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Core.Constants;

namespace DatabaseQuery.Views.Table
{
    public partial class FaiCollectionGridView : UserControl
    {
        public FaiCollectionGridView()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Show count of selected rows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionCount = PART_DataGrid.SelectedItems.Count;
        }


        /// <summary>
        /// Get all rows from datagrid
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;

            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }
        

        #region DictionaryUpperProperty

        public static readonly DependencyProperty DictionaryUpperProperty = DependencyProperty.Register(
            "DictionaryUpper", typeof(Dictionary<string, double>), typeof(FaiCollectionGridView),
            new PropertyMetadata(default(Dictionary<string, double>)));

        public Dictionary<string, double> DictionaryUpper
        {
            get { return (Dictionary<string, double>) GetValue(DictionaryUpperProperty); }
            set { SetValue(DictionaryUpperProperty, value); }
        }

        #endregion

        #region DictionaryLowerProperty

        public static readonly DependencyProperty DictionaryLowerProperty = DependencyProperty.Register(
            "DictionaryLower", typeof(Dictionary<string, double>), typeof(FaiCollectionGridView),
            new PropertyMetadata(default(Dictionary<string, double>)));

        public Dictionary<string, double> DictionaryLower
        {
            get { return (Dictionary<string, double>) GetValue(DictionaryLowerProperty); }
            set { SetValue(DictionaryLowerProperty, value); }
        }

        #endregion

        #region SelectionCountProperty

        public static readonly DependencyProperty SelectionCountProperty = DependencyProperty.Register(
            "SelectionCount", typeof(int), typeof(FaiCollectionGridView), new PropertyMetadata(default(int)));

        public int SelectionCount
        {
            get { return (int) GetValue(SelectionCountProperty); }
            set { SetValue(SelectionCountProperty, value); }
        }

        #endregion

        #region FaiCollectionsProperty

        public static readonly DependencyProperty FaiCollectionsProperty = DependencyProperty.Register(
            "FaiCollections", typeof(IEnumerable), typeof(FaiCollectionGridView),
            new PropertyMetadata(default(IEnumerable), OnFaiCollectionsChanged));

        private static void OnFaiCollectionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (FaiCollectionGridView) d;
            var collections = (IEnumerable) e.NewValue;
            if (collections == null) return;

            view.PART_DataGrid.ItemsSource = collections;
        }

        public IEnumerable FaiCollections
        {
            get { return (IEnumerable) GetValue(FaiCollectionsProperty); }
            set { SetValue(FaiCollectionsProperty, value); }
        }

        #endregion


        private void OnAutoGeneratedColumns(object sender, EventArgs e)
        {

        }

        private void OnDataGridMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (PART_DataGrid.DataContext == null || PART_DataGrid.ItemsSource == null) return;
            var rows = GetDataGridRows(PART_DataGrid);

            foreach (var row in rows)
            {
                foreach (var column in PART_DataGrid.Columns)
                {
                    var header = (string) column.Header;
                    if (column.GetCellContent(row) is TextBlock && header.Contains("FAI"))
                    {
                        TextBlock textBlock = (TextBlock) column.GetCellContent(row);
                        var value = double.Parse(textBlock.Text);
                        if (value > DictionaryUpper[header])
                            textBlock.Foreground = new SolidColorBrush(ColorConstants.ExceedUpperColor);
                        if (value < DictionaryLower[header])
                            textBlock.Foreground = new SolidColorBrush(ColorConstants.ExceedLowerColor);
                    }
                }
            }
        }

        private void OnDataGridMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (PART_DataGrid.DataContext == null || PART_DataGrid.ItemsSource == null) return;
            var rows = GetDataGridRows(PART_DataGrid);

            foreach (var row in rows)
            {
                foreach (var column in PART_DataGrid.Columns)
                {
                    var header = (string) column.Header;
                    if (column.GetCellContent(row) is TextBlock && header.Contains("FAI"))
                    {
                        TextBlock textBlock = (TextBlock) column.GetCellContent(row);
                        textBlock.Foreground = Brushes.Black;
                    }
                }
            }
        }
    }
}