using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace UI.Views.FaiItem
{
    public partial class FaiItemListStackView : UserControl
    {
        public FaiItemListStackView()
        {
            InitializeComponent();
        }
        
        public static readonly DependencyProperty FaiItemsProperty = DependencyProperty.Register(
            "FaiItems", typeof(IEnumerable<Core.ViewModels.Fai.FaiItem>), typeof(FaiItemListStackView),
            new PropertyMetadata(default(IEnumerable<Core.ViewModels.Fai.FaiItem>), OnFaiItemsChanged));

        private static void OnFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (FaiItemListStackView) d;
            var newValue = (IEnumerable<Core.ViewModels.Fai.FaiItem>) e.NewValue;
            if (newValue == null) return;

            sender.FaiList.ItemsSource = newValue;
        }

        public IEnumerable<Core.ViewModels.Fai.FaiItem> FaiItems
        {
            get { return (IEnumerable<Core.ViewModels.Fai.FaiItem>) GetValue(FaiItemsProperty); }
            set { SetValue(FaiItemsProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(string), typeof(FaiItemListStackView), new PropertyMetadata(default(string), OnHeaderChanged));

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (FaiItemListStackView) d;
            var newValue = (string) e.NewValue;
            if (newValue == null) return;
            sender.HeaderField.Text = newValue;
        }

        public string Header
        {
            get { return (string) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
    }
}