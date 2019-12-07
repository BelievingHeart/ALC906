using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Core.IoC.Loggers;
using Core.ViewModels.Fai;

namespace UI.Views.FaiItem.FaiItemsTable
{
    public partial class FaiTableStackView : UserControl
    {
        public FaiTableStackView()
        {
            InitializeComponent();
            DataContextChanged += OnViewModelChanged;
        }
        

        private void CreateTable(FaiTableStackViewModel viewModel)
        {
   
                // Clear grid
                PART_ItemsStack.Children.Clear();
            
                // Add header
                var headerContent = viewModel.Header;
                PART_HeaderRow.TextData = headerContent;

                // Add remaining rows
                foreach (var rowViewModel in viewModel.ValueMatrix)
                {
                    AddNewRow(rowViewModel);
                }
          
        }



        private void UnhookCallbacks(FaiTableStackViewModel table)
        {
            table.NewRowAdded -= AddNewRow;
            table.RowsRemoved -= OnRowsRemoved;
        }

        private void HookCallbacks(FaiTableStackViewModel table)
        {
            table.NewRowAdded += AddNewRow;
            table.RowsRemoved += OnRowsRemoved;
        }

        private void OnRowsRemoved()
        {
            CreateTable((FaiTableStackViewModel) DataContext);
        }

        private void AddNewRow(DataRowViewModel dataRowViewModel)
        {
      
                var newRow = new DataRowView {DataContext = dataRowViewModel, DataCellWidth = DataCellWidth};
                // Adjust header cells' width
                var binding = new Binding("ActualWidth")
                {
                    Source = newRow.PART_NameCell
                };
                PART_HeaderRow.PART_NameCell.SetBinding(WidthProperty, binding);
            
                // Add separator
                PART_ItemsStack.Children.Add(new Separator());
            
                // Add new row
                PART_ItemsStack.Children.Add(newRow);
          
        }
        
        

        private static void OnViewModelChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            var table = e.NewValue as FaiTableStackViewModel;
            if (table == null) return;

            var sender = (FaiTableStackView) o;
            // Hook callbacks to new fai table
            sender.HookCallbacks(table);

            // Create table
            sender.CreateTable(table);


            // Clear callbacks that is hooked to old fai table
            var oldTable =  e.OldValue as FaiTableStackViewModel;
            if (oldTable == null) return;
            sender.UnhookCallbacks(oldTable);
            
        }

        #region First Column Name

        public static readonly DependencyProperty FirstColumnNameProperty = DependencyProperty.Register(
            "FirstColumnName", typeof(string), typeof(FaiTableStackView), new PropertyMetadata(default(string), OnFirstColumnNameChanged));

        private static void OnFirstColumnNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (FaiTableStackView) d;
            var name = (string) e.NewValue;
            sender.PART_HeaderRow.RowName = name;
        }

        public string FirstColumnName
        {
            get { return (string) GetValue(FirstColumnNameProperty); }
            set { SetValue(FirstColumnNameProperty, value); }
        }

        #endregion

        #region Data Cell Width

        public static readonly DependencyProperty DataCellWidthProperty = DependencyProperty.Register(
            "DataCellWidth", typeof(double), typeof(FaiTableStackView), new PropertyMetadata(default(double), OnDataCellWidthChanged));

        private static void OnDataCellWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (FaiTableStackView) d;
            var width = (double) e.NewValue;

            sender.PART_HeaderRow.CellWidth = new GridLength(width);
        }

        public double DataCellWidth
        {
            get { return (double) GetValue(DataCellWidthProperty); }
            set { SetValue(DataCellWidthProperty, value); }
        }

        #endregion
    }
}