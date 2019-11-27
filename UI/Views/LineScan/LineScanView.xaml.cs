using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Enums;
using Core.ImageProcessing;
using UI.Views.Vision2D;
using WPFCommon.Commands;

namespace UI.Views.LineScan
{
    public partial class LineScanView : UserControl
    {
        public LineScanView()
        {
            InitializeComponent();
            SocketToDisplay = SocketType.Left;
            ChangeSocketViewCommand = new RelayCommand(SwitchSocketView);

        }
        
        private void SwitchSocketView()
        {
            SocketToDisplay = SocketToDisplay == SocketType.Left ? SocketType.Right : SocketType.Left;
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
            if(socket == SocketType.Left)
            {
                sender.ResultToDisplay = sender.Result3DLeft;
                sender.FaiItemsToDisplay = sender.LeftFaiItems;
            }
            else
            {
                sender.ResultToDisplay = sender.Result3DRight;
                sender.FaiItemsToDisplay = sender.RightFaiItems;
            }
        }

        public SocketType SocketToDisplay
        {
            get { return (SocketType) GetValue(SocketToDisplayProperty); }
            set { SetValue(SocketToDisplayProperty, value); }
        }

        public static readonly DependencyProperty ResultToDisplayProperty = DependencyProperty.Register(
            "ResultToDisplay", typeof(MeasurementResult3D), typeof(LineScanView),
            new PropertyMetadata(default(MeasurementResult3D)));

        public MeasurementResult3D ResultToDisplay
        {
            get { return (MeasurementResult3D) GetValue(ResultToDisplayProperty); }
            set { SetValue(ResultToDisplayProperty, value); }
        }

        public static readonly DependencyProperty Result3DLeftProperty = DependencyProperty.Register(
            "Result3DLeft", typeof(MeasurementResult3D), typeof(LineScanView),
            new PropertyMetadata(default(MeasurementResult3D), OnResult3DLeftChanged));

        private static void OnResult3DLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var leftResult = (MeasurementResult3D) e.NewValue;
            if (leftResult == null) return;
            var sender = d as LineScanView;
            if (sender.SocketToDisplay == SocketType.Left) sender.ResultToDisplay = leftResult;
        }
        public MeasurementResult3D Result3DLeft
        {
            get { return (MeasurementResult3D) GetValue(Result3DLeftProperty); }
            set { SetValue(Result3DLeftProperty, value); }
        }

        public static readonly DependencyProperty Result3DRightProperty = DependencyProperty.Register(
            "Result3DRight", typeof(MeasurementResult3D), typeof(LineScanView),
            new PropertyMetadata(default(MeasurementResult3D), OnResult3DRightChanged));

        private static void OnResult3DRightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rightResult = (MeasurementResult3D) e.NewValue;
            if (rightResult == null) return;
            var sender = d as LineScanView;
            if (sender.SocketToDisplay == SocketType.Right) sender.ResultToDisplay = rightResult;
        }

        public MeasurementResult3D Result3DRight
        {
            get { return (MeasurementResult3D) GetValue(Result3DRightProperty); }
            set { SetValue(Result3DRightProperty, value); }
        }

        
           #region FaiItems

        public static readonly DependencyProperty LeftFaiItemsProperty = DependencyProperty.Register(
            "LeftFaiItems", typeof(List<Core.ViewModels.Fai.FaiItem>), typeof(LineScanView), new PropertyMetadata(default(List<Core.ViewModels.Fai.FaiItem>), OnLeftFaiItemsChanged));

        private static void OnLeftFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as LineScanView;
            if (sender.SocketToDisplay == SocketType.Left) sender.FaiItemsToDisplay = (List<Core.ViewModels.Fai.FaiItem>) e.NewValue;
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
            if (sender.SocketToDisplay == SocketType.Right) sender.FaiItemsToDisplay = (List<Core.ViewModels.Fai.FaiItem>) e.NewValue;
        }

        public List<Core.ViewModels.Fai.FaiItem> RightFaiItems
        {
            get { return (List<Core.ViewModels.Fai.FaiItem>) GetValue(RightFaiItemsProperty); }
            set { SetValue(RightFaiItemsProperty, value); }
        }

        public static readonly DependencyProperty FaiItemsToDisplayProperty = DependencyProperty.Register(
            "FaiItemsToDisplay", typeof(List<Core.ViewModels.Fai.FaiItem>), typeof(LineScanView), new PropertyMetadata(default(List<Core.ViewModels.Fai.FaiItem>)));

        public List<Core.ViewModels.Fai.FaiItem> FaiItemsToDisplay
        {
            get { return (List<Core.ViewModels.Fai.FaiItem>) GetValue(FaiItemsToDisplayProperty); }
            set { SetValue(FaiItemsToDisplayProperty, value); }
        }
        

        #endregion
    
    }
}