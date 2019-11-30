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
            var index = (int) e.NewValue;
            sender.ImageToDisplay = sender.DisplayedResults2D.Images[index];
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
            var socket = (SocketType)e.NewValue;
            var sender = (Vision2DView) d;
            if (socket == SocketType.Left)
            {
                sender.DisplayedResults2D = sender.LeftResult2D;
            }
            else
            {
                sender.DisplayedResults2D = sender.RightResult2D;
            }
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
            if (sender.SocketToDisplay == SocketType.Left) sender.DisplayedResults2D = (MeasurementResult2D) e.NewValue;
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
            if (sender.SocketToDisplay == SocketType.Right) sender.DisplayedResults2D = (MeasurementResult2D) e.NewValue;
        }

        public MeasurementResult2D RightResult2D
        {
            get { return (MeasurementResult2D) GetValue(RightResult2DProperty); }
            set { SetValue(RightResult2DProperty, value); }
        }
        
        
        public static readonly DependencyProperty DisplayedResults2DProperty = DependencyProperty.Register(
            "DisplayedResults2D", typeof(MeasurementResult2D), typeof(Vision2DView), new PropertyMetadata(default(MeasurementResult2D), OnDisplayedResults2DChanged));
        
        private static void OnDisplayedResults2DChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newVal = e.NewValue as MeasurementResult2D;
            if (newVal == null) return;
            
            var sender = d as Vision2DView;
            // Init image index list
            if (sender.ImageIndexListToDisplay ==  null && newVal.Images!=null)
            {
                var indexList = new List<int>();
                for (int i = 0; i < newVal.Images.Count; i++)
                {
                    indexList.Add(i);
                }
                sender.ImageIndexListToDisplay = indexList;
            }
            // Update image to display
            if (newVal.Images != null) sender.ImageToDisplay = newVal.Images[sender.ImageIndexToDisplay];
        }

        public MeasurementResult2D DisplayedResults2D
        {
            get { return (MeasurementResult2D) GetValue(DisplayedResults2DProperty); }
            set { SetValue(DisplayedResults2DProperty, value); }
        }
        
        #endregion
        

        private void OnVision2DViewLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.WindowHandle2D = HalconScreen.HalconWindow;
        }
    }
}