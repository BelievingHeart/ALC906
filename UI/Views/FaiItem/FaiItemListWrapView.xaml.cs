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
            "FaiItems", typeof(IList<Core.ViewModels.Fai.FaiItem>), typeof(FaiItemListWarpView),
            new PropertyMetadata(default(IList<Core.ViewModels.Fai.FaiItem>)));

       public IList<Core.ViewModels.Fai.FaiItem> FaiItems
        {
            get { return (IList<Core.ViewModels.Fai.FaiItem>) GetValue(FaiItemsProperty); }
            set { SetValue(FaiItemsProperty, value); }
        }
        
    }
}