using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UI.Views.FaiItem
{
    public partial class FaiItemListWarpView : UserControl
    {
        public FaiItemListWarpView()
        {
            InitializeComponent();
        }
 
       public static readonly DependencyProperty FaiItemsProperty = DependencyProperty.Register(
            "FaiItems", typeof(IEnumerable<Core.ViewModels.Fai.FaiItem>), typeof(FaiItemListWarpView),
            new PropertyMetadata(default(IEnumerable<Core.ViewModels.Fai.FaiItem>), OnFaiItemsChanged));

       private static void OnFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
       {
           var view = (FaiItemListWarpView) d;
           var items = (IEnumerable<Core.ViewModels.Fai.FaiItem>) e.NewValue;
           var text = items.Any(item=>item.Rejected)? "NG" : "OK";
           view.PART_TextBlockOverallResult.Text = text;
           view.PART_TextBlockOverallResult.Foreground = new SolidColorBrush(text == "OK" ? Colors.Green : Colors.Red);
       }

       public IEnumerable<Core.ViewModels.Fai.FaiItem> FaiItems
        {
            get { return (IEnumerable<Core.ViewModels.Fai.FaiItem>) GetValue(FaiItemsProperty); }
            set { SetValue(FaiItemsProperty, value); }
        }
        
    }
}