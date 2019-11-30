using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace UI.Views.FaiItem
{
    public partial class FaiItemGridView : UserControl
    {
        public FaiItemGridView()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty FaiItemsProperty = DependencyProperty.Register(
            "FaiItems", typeof(IEnumerable<Core.ViewModels.Fai.FaiItem>), typeof(FaiItemGridView), new PropertyMetadata(default(IEnumerable<Core.ViewModels.Fai.FaiItem>), OnFaiItemsChanged));

        private static void OnFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (FaiItemGridView) d;
            var newValue = (IEnumerable<Core.ViewModels.Fai.FaiItem>) e.NewValue;
            if (newValue == null) return;
            sender.PART_DataGrid.ItemsSource = newValue;
        }

        public IEnumerable<Core.ViewModels.Fai.FaiItem> FaiItems
        {
            get { return (IEnumerable<Core.ViewModels.Fai.FaiItem>) GetValue(FaiItemsProperty); }
            set { SetValue(FaiItemsProperty, value); }
        }
    }
}