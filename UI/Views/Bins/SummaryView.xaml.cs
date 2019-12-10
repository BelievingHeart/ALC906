using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core.Enums;
using Core.ViewModels.Application;
using PLS;

namespace UI.Views.Bins
{
    public partial class SummaryView : UserControl
    {
        public SummaryView()
        {
            InitializeComponent();
        }

        private void OnSummarySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedText = (string) PART_SummarySelectionComboBox.SelectedItem;
            ItemToDisplay = selectedText == "Today" ? Summary.TodaySummaryItem : Summary.AllSummaryItems.First(s => s.SummaryName == selectedText);
        }

      

        #region Summary

        public static readonly DependencyProperty SummaryProperty = DependencyProperty.Register(
            "Summary", typeof(ProductionLineSummary), typeof(SummaryView),
            new PropertyMetadata(default(ProductionLineSummary), OnSummaryChanged));

        private static void OnSummaryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (SummaryView) d;
            var summary = (ProductionLineSummary) e.NewValue;
            if (summary == null) return;

            // Initial display 
            sender.RefreshSummaryItemList();
            sender.PART_SummarySelectionComboBox.SelectedIndex = 0;

            // Hook event handlers to new summary
            sender.HookEventsToSummary(summary);

            // Unhook event handlers to old summary
            var oldSummary = (ProductionLineSummary) e.OldValue;
            if (oldSummary == null) return;
            sender.UnhookEventsToSummary(oldSummary);
        }

        private void UnhookEventsToSummary(ProductionLineSummary oldSummary)
        {
            oldSummary.HourChanged -= RefreshSummaryItemList;
        }

  

        private void RefreshSummaryItemList()
        {
            var options = Summary.AllSummaryItems.Select(items => items.SummaryName).ToList();
            options.Add("Today");
            PART_SummarySelectionComboBox.ItemsSource = options;
        }

        private void HookEventsToSummary(ProductionLineSummary summary)
        {
            summary.HourChanged += RefreshSummaryItemList;
        }



        public ProductionLineSummary Summary
        {
            get { return (ProductionLineSummary) GetValue(SummaryProperty); }
            set { SetValue(SummaryProperty, value); }
        }

        #endregion


        #region ItemToDisplayProperty

        public static readonly DependencyProperty ItemToDisplayProperty = DependencyProperty.Register(
            "ItemToDisplay", typeof(ProductionLineSummaryItem), typeof(SummaryView), new PropertyMetadata(default(ProductionLineSummaryItem), OnItemsToDisplayChanged));

        private static void OnItemsToDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (SummaryView) d;
            var newItem = (ProductionLineSummaryItem) e.NewValue;
            if (newItem == null) return;
            
            // Initial display
            sender.DisplaySummaryItem(newItem.BinsAndCounts);
            newItem.OnPropertyChanged(nameof(ProductionLineSummaryItem.NgCount));
            var propName = nameof(newItem.NgCount);
            newItem.OnPropertyChanged(propName);

            // Hook event handler to new item
            newItem.BinsIncremented += sender.DisplaySummaryItem;

            // Unhook event handler to old item
            var oldItem = (ProductionLineSummaryItem) e.OldValue;
            if (oldItem == null) return;
            oldItem.BinsIncremented -= sender.DisplaySummaryItem;
        }

        public ProductionLineSummaryItem ItemToDisplay
        {
            get { return (ProductionLineSummaryItem) GetValue(ItemToDisplayProperty); }
            set { SetValue(ItemToDisplayProperty, value); }
        }


        #endregion


        /// <summary>
        /// Display the bin and count of current summary if current summary is selected
        /// otherwise
        /// </summary>
        /// <param name="binsAndCounts"></param>
        private void DisplaySummaryItem(Dictionary<string, int> binsAndCounts)
        {
            PART_OkCountTextBlock.Text = binsAndCounts[ProductLevel.OK.ToString()].ToString();
            PART_Ng2CountLabel.Content = binsAndCounts[ProductLevel.Ng2.ToString()];
            PART_Ng3CountLabel.Content = binsAndCounts[ProductLevel.Ng3.ToString()];
            PART_Ng4CountLabel.Content = binsAndCounts[ProductLevel.Ng4.ToString()];
            PART_Ng5Count.Content = binsAndCounts[ProductLevel.Ng5.ToString()];
            PART_NgCountTextBlock.Text = ItemToDisplay.NgCount.ToString();
            PART_UphTextBlock.Text = ItemToDisplay.UnitsPerHour.ToString();
            PART_YieldTextBlock.Text = $"{ItemToDisplay.Yield * 100:N1}%";
        }

        
    }
}