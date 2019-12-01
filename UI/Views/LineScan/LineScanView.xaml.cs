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
            sender.UpdateGraphics(socket, sender.ImageIndex);
        }

        public SocketType SocketToDisplay
        {
            get { return (SocketType) GetValue(SocketToDisplayProperty); }
            set { SetValue(SocketToDisplayProperty, value); }
        }

        public static readonly DependencyProperty GraphicPackLeftProperty = DependencyProperty.Register(
            "GraphicPackLeft", typeof(GraphicPack3DViewModel), typeof(LineScanView),
            new PropertyMetadata(default(GraphicPack3DViewModel), OnResult3DLeftChanged));

        private static void OnResult3DLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var leftResult = (GraphicPack3DViewModel) e.NewValue;
            if (leftResult == null) return;
            var sender = d as LineScanView;
            if (sender.SocketToDisplay == SocketType.Left) sender.UpdateGraphics(sender.SocketToDisplay, sender.ImageIndex);
        }
        public GraphicPack3DViewModel GraphicPackLeft
        {
            get { return (GraphicPack3DViewModel) GetValue(GraphicPackLeftProperty); }
            set { SetValue(GraphicPackLeftProperty, value); }
        }

        public static readonly DependencyProperty GraphicPackRightProperty = DependencyProperty.Register(
            "GraphicPackRight", typeof(GraphicPack3DViewModel), typeof(LineScanView),
            new PropertyMetadata(default(GraphicPack3DViewModel), OnResult3DRightChanged));

        private static void OnResult3DRightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rightResult = (GraphicPack3DViewModel) e.NewValue;
            if (rightResult == null) return;
            var sender = d as LineScanView;
            if (sender.SocketToDisplay == SocketType.Right) sender.UpdateGraphics(sender.SocketToDisplay, sender.ImageIndex);
        }

        public GraphicPack3DViewModel GraphicPackRight
        {
            get { return (GraphicPack3DViewModel) GetValue(GraphicPackRightProperty); }
            set { SetValue(GraphicPackRightProperty, value); }
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

        public static readonly DependencyProperty FaiItemsToDisplayProperty = DependencyProperty.Register(
            "FaiItemsToDisplay", typeof(List<Core.ViewModels.Fai.FaiItem>), typeof(LineScanView), new PropertyMetadata(default(List<Core.ViewModels.Fai.FaiItem>), OnFaiItemsToDisplayChanged));

        private static void OnFaiItemsToDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as LineScanView;
            var newValue = (List<Core.ViewModels.Fai.FaiItem>) e.NewValue;
            if (newValue == null) return;
            sender.PART_FaiItemGridView.FaiItems = newValue;
        }

  
        

        #endregion


        public static readonly DependencyProperty ImageIndexProperty = DependencyProperty.Register(
            "ImageIndex", typeof(int), typeof(LineScanView), new PropertyMetadata(default(int), OnImageIndexChanged));

        private static void OnImageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as LineScanView;
            var newValue = (int) e.NewValue;
            sender?.UpdateGraphics(sender.SocketToDisplay, newValue);
        }

        public int ImageIndex
        {
            get { return (int) GetValue(ImageIndexProperty); }
            set { SetValue(ImageIndexProperty, value); }
        }
        

        private HWindow _windowHandle;
        private void LineScanView_OnLoaded(object sender, RoutedEventArgs e)
        {
            _windowHandle = PART_HalconWindow.HalconWindow;
            _windowHandle.SetPart(0, 0, -2, -2);
            _windowHandle.SetColor("green");
        }

        private void UpdateGraphics(SocketType socketType, int imageIndex)
        {
            var graphics = socketType == SocketType.Left ? GraphicPackLeft : GraphicPackRight;
            if (graphics == null) return;

            _windowHandle.DispImage(graphics.Images[imageIndex]);
            _windowHandle.DispObj(graphics.Graphics);
            if(graphics.ErrorMessage!=null) _windowHandle.DispText(graphics.ErrorMessage, "window", "center", "center", "red", "box", 12);
        }

        private void UpdateFaiItemsView(List<Core.ViewModels.Fai.FaiItem> faiItems)
        {
            PART_FaiItemGridView.FaiItems = faiItems;
        }
    }
}