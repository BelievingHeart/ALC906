﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            new PropertyMetadata(default(IEnumerable<Core.ViewModels.Fai.FaiItem>)));

        public IEnumerable<Core.ViewModels.Fai.FaiItem> FaiItems
        {
            get { return (IEnumerable<Core.ViewModels.Fai.FaiItem>) GetValue(FaiItemsProperty); }
            set { SetValue(FaiItemsProperty, value); }
        }
        
    }
}