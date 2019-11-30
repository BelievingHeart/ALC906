using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Enums;
using Core.ImageProcessing;
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
                sender.DisplayedFaiItems = sender.LeftFaiItems;
                sender.DisplayedResults2D = sender.LeftResult2D;
            }
            else
            {
                sender.DisplayedFaiItems = sender.RightFaiItems;
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

        
        
        #region FaiItems

        public static readonly DependencyProperty LeftFaiItemsProperty = DependencyProperty.Register(
            "LeftFaiItems", typeof(List<Core.ViewModels.Fai.FaiItem>), typeof(Vision2DView), new PropertyMetadata(default(List<Core.ViewModels.Fai.FaiItem>), OnLeftFaiItemsChanged));

        private static void OnLeftFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as Vision2DView;
            if (sender.SocketToDisplay == SocketType.Left) sender.DisplayedFaiItems = (List<Core.ViewModels.Fai.FaiItem>) e.NewValue;
        }


        public List<Core.ViewModels.Fai.FaiItem> LeftFaiItems
        {
            get { return (List<Core.ViewModels.Fai.FaiItem>) GetValue(LeftFaiItemsProperty); }
            set { SetValue(LeftFaiItemsProperty, value); }
        }

        public static readonly DependencyProperty RightFaiItemsProperty = DependencyProperty.Register(
            "RightFaiItems", typeof(List<Core.ViewModels.Fai.FaiItem>), typeof(Vision2DView), new PropertyMetadata(default(List<Core.ViewModels.Fai.FaiItem>), OnRightFaiItemsChanged));

        private static void OnRightFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as Vision2DView;
            if (sender.SocketToDisplay == SocketType.Right) sender.DisplayedFaiItems = (List<Core.ViewModels.Fai.FaiItem>) e.NewValue;
        }

        public List<Core.ViewModels.Fai.FaiItem> RightFaiItems
        {
            get { return (List<Core.ViewModels.Fai.FaiItem>) GetValue(RightFaiItemsProperty); }
            set { SetValue(RightFaiItemsProperty, value); }
        }

        public static readonly DependencyProperty DisplayedFaiItemsProperty = DependencyProperty.Register(
            "DisplayedFaiItems", typeof(List<Core.ViewModels.Fai.FaiItem>), typeof(Vision2DView), new PropertyMetadata(default(List<Core.ViewModels.Fai.FaiItem>), OnDisplayedFaiItemsChanged));

        private static void OnDisplayedFaiItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (Vision2DView) d;
            var newValue = (List<Core.ViewModels.Fai.FaiItem>) e.NewValue;
            if (newValue == null) return;
            sender.PART_FaiItemsGridView.FaiItems = newValue;
        }

        public List<Core.ViewModels.Fai.FaiItem> DisplayedFaiItems
        {
            get { return (List<Core.ViewModels.Fai.FaiItem>) GetValue(DisplayedFaiItemsProperty); }
            set { SetValue(DisplayedFaiItemsProperty, value); }
        }
        

        #endregion

        private void OnFindLineParamGridAutoGenerateColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var header = (string)e.Column.Header;
            if (header == "SerializationDirectory" || header == "ShouldAutoSerialize" || header == "AutoResetFlag" || header == "AutoResetInterval") e.Cancel = true;
        }
    }
}