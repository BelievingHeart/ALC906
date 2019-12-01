using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Enums;
using Core.ImageProcessing;
using Core.ViewModels.Application;
using HalconDotNet;
using WPFCommon.Commands;

namespace UI.Views.Vision2D
{
    /// <summary>
    /// View for individual camera
    /// </summary>
    public partial class Vision2DView : UserControl
    {
        public Vision2DView()
        {
            InitializeComponent();
            ChangeSocketViewCommand = new RelayCommand(SwitchSocketView);
            SocketToDisplay = SocketType.Left;
        }

        private void SwitchSocketView()
        {
            SocketToDisplay = SocketToDisplay == SocketType.Left ? SocketType.Right : SocketType.Left;
        }

        public static readonly DependencyProperty ImageIndexToDisplayProperty = DependencyProperty.Register(
            "ImageIndexToDisplay", typeof(int), typeof(Vision2DView), new PropertyMetadata(default(int), OnImageIndexToDisplayChanged));

        private static void OnImageIndexToDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as Vision2DView;
            sender.RefreshHalconScreen();
        }

        public int ImageIndexToDisplay
        {
            get { return (int) GetValue(ImageIndexToDisplayProperty); }
            set { SetValue(ImageIndexToDisplayProperty, value); }
        }

        public static readonly DependencyProperty ImageIndexListToDisplayProperty = DependencyProperty.Register(
            "ImageIndexListToDisplay", typeof(List<int>), typeof(Vision2DView), new PropertyMetadata(default(List<int>)));

        public List<int> ImageIndexListToDisplay
        {
            get { return (List<int>) GetValue(ImageIndexListToDisplayProperty); }
            set { SetValue(ImageIndexListToDisplayProperty, value); }
        }


        public static readonly DependencyProperty ImageToDisplayProperty = DependencyProperty.Register(
            "ImageToDisplay", typeof(HImage), typeof(Vision2DView), new PropertyMetadata(default(HImage)));

        public HImage ImageToDisplay
        {
            get { return (HImage) GetValue(ImageToDisplayProperty); }
            set { SetValue(ImageToDisplayProperty, value); }
        }
        
        

        public static readonly DependencyProperty ChangeSocketViewCommandProperty = DependencyProperty.Register(
            "ChangeSocketViewCommand", typeof(ICommand), typeof(Vision2DView), new PropertyMetadata(default(ICommand)));

        public ICommand ChangeSocketViewCommand
        {
            get { return (ICommand) GetValue(ChangeSocketViewCommandProperty); }
            set { SetValue(ChangeSocketViewCommandProperty, value); }
        }


        public static readonly DependencyProperty SocketToDisplayProperty = DependencyProperty.Register(
            "SocketToDisplay", typeof(SocketType), typeof(Vision2DView), new PropertyMetadata(default(SocketType), OnSocketToDisplayChanged));

        private static void OnSocketToDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (Vision2DView) d;
            sender.RefreshHalconScreen();
        }

        public SocketType SocketToDisplay
        {
            get { return (SocketType) GetValue(SocketToDisplayProperty); }
            set { SetValue(SocketToDisplayProperty, value); }
        }

        #region Results2D

        public static readonly DependencyProperty LeftResult2DProperty = DependencyProperty.Register(
            "LeftResult2D", typeof(MeasurementResult2D), typeof(Vision2DView), new PropertyMetadata(default(MeasurementResult2D), OnLeftResult2DChanged));

        private static void OnLeftResult2DChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as Vision2DView;
            if (sender.SocketToDisplay == SocketType.Left) sender.RefreshHalconScreen();
        }


        public MeasurementResult2D LeftResult2D
        {
            get { return (MeasurementResult2D) GetValue(LeftResult2DProperty); }
            set { SetValue(LeftResult2DProperty, value); }
        }
        
        

        public static readonly DependencyProperty RightResult2DProperty = DependencyProperty.Register(
            "RightResult2D", typeof(MeasurementResult2D), typeof(Vision2DView), new PropertyMetadata(default(MeasurementResult2D), OnRightResult2DChanged));

        private static void OnRightResult2DChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as Vision2DView;
            if (sender.SocketToDisplay == SocketType.Right) sender.RefreshHalconScreen();
        }

        public MeasurementResult2D RightResult2D
        {
            get { return (MeasurementResult2D) GetValue(RightResult2DProperty); }
            set { SetValue(RightResult2DProperty, value); }
        }
        
        #endregion

        private HWindow _windowHandle;
        
        private void OnVision2DViewLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.WindowHandle2D = HalconScreen.HalconWindow;
            _windowHandle = HalconScreen.HalconWindow;
            _windowHandle.SetColored(3);
            _windowHandle.SetPart(0,0,-2,-2);
            _windowHandle.SetColor("green");
        }

        private void UpdateImageIndexList(MeasurementResult2D result2D)
        {
            // Init image index list
            if (ImageIndexListToDisplay !=  null &&  result2D.Images!=null)
            {
                var indexList = new List<int>();
                for (int i = 0; i < result2D.Images.Count; i++)
                {
                    indexList.Add(i);
                }
                ImageIndexListToDisplay = indexList;
            }
        }
        
        private void RefreshHalconScreen()
        {
            var result2D = SocketToDisplay == SocketType.Left ? LeftResult2D : RightResult2D;
            UpdateImageIndexList(result2D);
            var image = result2D.Images[ImageIndexToDisplay];
            var graphics = result2D.Graphics;
            _windowHandle.DispImage(image);
            _windowHandle.DispObj(graphics);
        }
    }
}