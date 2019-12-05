using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Enums;
using Core.ImageProcessing;
using Core.ViewModels.Results;
using HalconDotNet;
using UI.Views.Vision2D;
using WPFCommon.Commands;

namespace UI.Views.LineScan
{
    public partial class LineScanView : UserControl
    {
        public LineScanView()
        {
            InitializeComponent();
   

        }
        
        public static readonly DependencyProperty ChangeSocketViewCommandProperty = DependencyProperty.Register(
            "ChangeSocketViewCommand", typeof(ICommand), typeof(LineScanView), new PropertyMetadata(default(ICommand)));

        public ICommand ChangeSocketViewCommand
        {
            get { return (ICommand) GetValue(ChangeSocketViewCommandProperty); }
            set { SetValue(ChangeSocketViewCommandProperty, value); }
        }

        public static readonly DependencyProperty SocketToDisplayProperty = DependencyProperty.Register(
            "SocketToDisplay", typeof(SocketType), typeof(LineScanView), new PropertyMetadata(default(SocketType), OnSocketToDisplayChanged));

        private static void OnSocketToDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var socket = (SocketType) e.NewValue;
            var sender = d as LineScanView;
            sender.UpdateFaiItemsView(socket == SocketType.Left? sender.LeftFaiItems : sender.RightFaiItems);
        }

        public SocketType SocketToDisplay
        {
            get { return (SocketType) GetValue(SocketToDisplayProperty); }
            set { SetValue(SocketToDisplayProperty, value); }
        }
        
           #region FaiItems

        public static readonly DependencyProperty LeftFaiItemsProperty = DependencyProperty.Register(
            "LeftFaiItems", typeof(List<Core.ViewModels.Fai.FaiItem>), typeof(LineScanView), new PropertyMetadata(default(List<Core.ViewModels.Fai.FaiItem>), OnLeftFaiItemsChanged));

        private static void OnLeftFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as LineScanView;
            var newValue = (List<Core.ViewModels.Fai.FaiItem>) e.NewValue;
            if (newValue == null) return;
            if (sender.SocketToDisplay == SocketType.Left) sender.UpdateFaiItemsView(newValue);
        }


        public List<Core.ViewModels.Fai.FaiItem> LeftFaiItems
        {
            get { return (List<Core.ViewModels.Fai.FaiItem>) GetValue(LeftFaiItemsProperty); }
            set { SetValue(LeftFaiItemsProperty, value); }
        }

        public static readonly DependencyProperty RightFaiItemsProperty = DependencyProperty.Register(
            "RightFaiItems", typeof(List<Core.ViewModels.Fai.FaiItem>), typeof(LineScanView), new PropertyMetadata(default(List<Core.ViewModels.Fai.FaiItem>), OnRightFaiItemsChanged));

        private static void OnRightFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as LineScanView;
            var newValue = (List<Core.ViewModels.Fai.FaiItem>) e.NewValue;
            if (newValue == null) return;
            if (sender.SocketToDisplay == SocketType.Right) sender.UpdateFaiItemsView(newValue);
        }

        public List<Core.ViewModels.Fai.FaiItem> RightFaiItems
        {
            get { return (List<Core.ViewModels.Fai.FaiItem>) GetValue(RightFaiItemsProperty); }
            set { SetValue(RightFaiItemsProperty, value); }
        }

        
        #endregion



   

        private void UpdateFaiItemsView(List<Core.ViewModels.Fai.FaiItem> faiItems)
        {
            PART_FaiItemGridView.LeftFaiItems = faiItems;
        }
    }
}