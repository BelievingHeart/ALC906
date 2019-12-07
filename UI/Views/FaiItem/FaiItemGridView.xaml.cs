using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Core.Enums;
using Core.ViewModels.Application;

namespace UI.Views.FaiItem
{
    public partial class FaiItemGridView : UserControl
    {
        public FaiItemGridView()
        {
            InitializeComponent();
        }


        #region LeftFaiItems

        public static readonly DependencyProperty LeftFaiItemsProperty = DependencyProperty.Register(
            "LeftFaiItems", typeof(IEnumerable<Core.ViewModels.Fai.FaiItem>), typeof(FaiItemGridView), new PropertyMetadata(default(IEnumerable<Core.ViewModels.Fai.FaiItem>), OnLeftFaiItemsChanged));

        public IEnumerable<Core.ViewModels.Fai.FaiItem> LeftFaiItems
        {
            get { return (IEnumerable<Core.ViewModels.Fai.FaiItem>) GetValue(LeftFaiItemsProperty); }
            set { SetValue(LeftFaiItemsProperty, value); }
        }
        
        private static void OnLeftFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (FaiItemGridView) d;
            var newValue = (IEnumerable<Core.ViewModels.Fai.FaiItem>) e.NewValue;
            if (newValue != null && sender.SocketType == SocketType.Cavity1) sender.PART_DataGrid.ItemsSource = newValue;
        }

        #endregion


        #region RightFaiItems

        public static readonly DependencyProperty RightFaiItemsProperty = DependencyProperty.Register(
            "RightFaiItems", typeof(IEnumerable<Core.ViewModels.Fai.FaiItem>), typeof(FaiItemGridView), new PropertyMetadata(default(IEnumerable<Core.ViewModels.Fai.FaiItem>), OnRightFaiItemsChanged));

        public IEnumerable<Core.ViewModels.Fai.FaiItem> RightFaiItems
        {
            get { return (IEnumerable<Core.ViewModels.Fai.FaiItem>) GetValue(RightFaiItemsProperty); }
            set { SetValue(RightFaiItemsProperty, value); }
        }
        
        private static void OnRightFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (FaiItemGridView) d;
            var newValue = (IEnumerable<Core.ViewModels.Fai.FaiItem>) e.NewValue;
            if (newValue != null && sender.SocketType == SocketType.Cavity2) sender.PART_DataGrid.ItemsSource = newValue;
        }
        

        #endregion


        #region SocketToDisplay

        public static readonly DependencyProperty SocketTypeProperty = DependencyProperty.Register(
            "SocketType", typeof(SocketType), typeof(FaiItemGridView), new PropertyMetadata(default(SocketType), OnSocketTypeChanged));

  
        public SocketType SocketType
        {
            get { return (SocketType) GetValue(SocketTypeProperty); }
            set { SetValue(SocketTypeProperty, value); }
        }
        
        private static void OnSocketTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (FaiItemGridView) d;
            var newSocketType = (SocketType) e.NewValue;
            var itemsSource = newSocketType == SocketType.Cavity1 ? sender.LeftFaiItems : sender.RightFaiItems;
            sender.PART_DataGrid.ItemsSource = itemsSource;
        }

        #endregion

        
    }
}