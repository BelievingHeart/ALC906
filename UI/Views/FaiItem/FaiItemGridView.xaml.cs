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


        public static readonly DependencyProperty LeftFaiItemsProperty = DependencyProperty.Register(
            "LeftFaiItems", typeof(IEnumerable<Core.ViewModels.Fai.FaiItem>), typeof(FaiItemGridView), new PropertyMetadata(default(IEnumerable<Core.ViewModels.Fai.FaiItem>), OnLeftFaiItemsChanged));

        private static void OnLeftFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //TODO: remove this
            ApplicationViewModel.Instance.LogRoutine("OnLeftFaiItemsChanged");
            
            var sender = (FaiItemGridView) d;
            var newValue = (IEnumerable<Core.ViewModels.Fai.FaiItem>) e.NewValue;
            if (newValue == null && sender.SocketType == SocketType.Left) return;
            sender.PART_DataGrid.ItemsSource = newValue;
        }

        public IEnumerable<Core.ViewModels.Fai.FaiItem> LeftFaiItems
        {
            get { return (IEnumerable<Core.ViewModels.Fai.FaiItem>) GetValue(LeftFaiItemsProperty); }
            set { SetValue(LeftFaiItemsProperty, value); }
        }
        
        public static readonly DependencyProperty RightFaiItemsProperty = DependencyProperty.Register(
            "RightFaiItems", typeof(IEnumerable<Core.ViewModels.Fai.FaiItem>), typeof(FaiItemGridView), new PropertyMetadata(default(IEnumerable<Core.ViewModels.Fai.FaiItem>), OnRightFaiItemsChanged));

        private static void OnRightFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (FaiItemGridView) d;
            var newValue = (IEnumerable<Core.ViewModels.Fai.FaiItem>) e.NewValue;
            if (newValue == null && sender.SocketType == SocketType.Right) return;
            sender.PART_DataGrid.ItemsSource = newValue;
        }

        public IEnumerable<Core.ViewModels.Fai.FaiItem> RightFaiItems
        {
            get { return (IEnumerable<Core.ViewModels.Fai.FaiItem>) GetValue(RightFaiItemsProperty); }
            set { SetValue(RightFaiItemsProperty, value); }
        }

        public static readonly DependencyProperty SocketTypeProperty = DependencyProperty.Register(
            "SocketType", typeof(SocketType), typeof(FaiItemGridView), new PropertyMetadata(default(SocketType), OnSocketTypeChanged));

        private static void OnSocketTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (FaiItemGridView) d;
            var newSocketType = (Core.Enums.SocketType) e.NewValue;
            sender.PART_DataGrid.ItemsSource =
                newSocketType == SocketType.Left ? sender.LeftFaiItems : sender.RightFaiItems;
        }

        public SocketType SocketType
        {
            get { return (SocketType) GetValue(SocketTypeProperty); }
            set { SetValue(SocketTypeProperty, value); }
        }
    }
}