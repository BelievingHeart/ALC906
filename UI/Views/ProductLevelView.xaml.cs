using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Enums;
using static System.String;

namespace UI.Views
{
    public partial class ProductLevelView : UserControl
    {
        public ProductLevelView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ProductLevelProperty = DependencyProperty.Register(
            "ProductLevel", typeof(ProductLevel), typeof(ProductLevelView), new PropertyMetadata(default(ProductLevel), OnProductLevelChanged));

        private static void OnProductLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ProductLevelView) d;
            var productLevel = (ProductLevel) e.NewValue;
            // Avoid recursive call
            if (productLevel == ProductLevel.Undefined) return;
            view.DisplayText = Empty;

            if (productLevel == ProductLevel.OK) {
                view.DisplayText = "OK";
                view.PART_ProductLevelTextBlock.Foreground = Brushes.Green;
            }
            else if (productLevel == ProductLevel.Ng2 || productLevel == ProductLevel.Ng3 ||
                     productLevel == ProductLevel.Ng4) {
                view.DisplayText = "NG";
                view.PART_ProductLevelTextBlock.Foreground = Brushes.Red;
            }
            else if (productLevel == ProductLevel.Ng5) {
                view.DisplayText = "错误";
                view.PART_ProductLevelTextBlock.Foreground = Brushes.Goldenrod;
            }
            else if (productLevel == ProductLevel.Empty) {
                view.DisplayText = "空";
                view.PART_ProductLevelTextBlock.Foreground = Brushes.Blue;
            }
            
            // Allow product level to be reassigned
            view.ProductLevel = ProductLevel.Undefined;
        }

        public ProductLevel ProductLevel
        {
            get { return (ProductLevel) GetValue(ProductLevelProperty); }
            set { SetValue(ProductLevelProperty, value); }
        }

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register(
            "DisplayText", typeof(string), typeof(ProductLevelView), new PropertyMetadata(default(string)));

        public string DisplayText
        {
            get { return (string) GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }
    }
}