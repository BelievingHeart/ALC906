using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Enums;
using Core.ViewModels.Results;
using HalconDotNet;
using UI.Views.Vision2D;
using WPFCommon.Commands;

namespace UI.Views.HalconScreen
{
    public partial class HalconScreenView : UserControl
    {
        public HalconScreenView()
        {
            InitializeComponent();
        }

        private void OnHalconWindowLoaded(object sender, RoutedEventArgs e)
        {
            ChangeSocketViewCommand = new RelayCommand(SwitchSocketView);
            SocketToDisplay = SocketType.Left;
            _windowHandle = HalconScreen.HalconWindow;
            _windowHandle.SetColored(3);
            _windowHandle.SetPart(0,0,-2,-2);
            _windowHandle.SetColor("green");
        }

        public static readonly DependencyProperty ButtonVisibilityProperty = DependencyProperty.Register(
            "ButtonVisibility", typeof(Visibility), typeof(HalconScreenView), new PropertyMetadata(System.Windows.Visibility.Visible, OnButtonVisibilityChanged));

        private static void OnButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (HalconScreenView) d;
            var newValue = (Visibility) e.NewValue;
            sender.PART_SwitchViewButton.Visibility = newValue;
            sender.PART_SelectImageComboBox.Visibility = newValue;
        }

        public Visibility ButtonVisibility
        {
            get { return (Visibility) GetValue(ButtonVisibilityProperty); }
            set { SetValue(ButtonVisibilityProperty, value); }
        }
        
        private void SwitchSocketView()
        {
            SocketToDisplay = SocketToDisplay == SocketType.Left ? SocketType.Right : SocketType.Left;
        }

        public static readonly DependencyProperty ImageIndexToDisplayProperty = DependencyProperty.Register(
            "ImageIndexToDisplay", typeof(int), typeof(HalconScreenView), new PropertyMetadata(default(int), OnImageIndexToDisplayChanged));

        private static void OnImageIndexToDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as HalconScreenView;
            sender.RefreshHalconScreen();
        }

        public int ImageIndexToDisplay
        {
            get { return (int) GetValue(ImageIndexToDisplayProperty); }
            set { SetValue(ImageIndexToDisplayProperty, value); }
        }

        public static readonly DependencyProperty ImageIndexListToDisplayProperty = DependencyProperty.Register(
            "ImageIndexListToDisplay", typeof(List<int>), typeof(HalconScreenView), new PropertyMetadata(default(List<int>)));

        public List<int> ImageIndexListToDisplay
        {
            get { return (List<int>) GetValue(ImageIndexListToDisplayProperty); }
            set { SetValue(ImageIndexListToDisplayProperty, value); }
        }
        
           public static readonly DependencyProperty ChangeSocketViewCommandProperty = DependencyProperty.Register(
            "ChangeSocketViewCommand", typeof(ICommand), typeof(HalconScreenView), new PropertyMetadata(default(ICommand)));

        public ICommand ChangeSocketViewCommand
        {
            get { return (ICommand) GetValue(ChangeSocketViewCommandProperty); }
            set { SetValue(ChangeSocketViewCommandProperty, value); }
        }


        public static readonly DependencyProperty SocketToDisplayProperty = DependencyProperty.Register(
            "SocketToDisplay", typeof(SocketType), typeof(HalconScreenView), new PropertyMetadata(default(SocketType), OnSocketToDisplayChanged));

        private static void OnSocketToDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (HalconScreenView) d;
            sender.RefreshHalconScreen();
        }

        public SocketType SocketToDisplay
        {
            get { return (SocketType) GetValue(SocketToDisplayProperty); }
            set { SetValue(SocketToDisplayProperty, value); }
        }

        #region Results2D

        public static readonly DependencyProperty LeftGraphicsProperty = DependencyProperty.Register(
            "LeftGraphics", typeof(GraphicsPackViewModel), typeof(HalconScreenView), new PropertyMetadata(default(GraphicsPackViewModel), OnLeftResult2DChanged));

        private static void OnLeftResult2DChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as HalconScreenView;
            if (sender.SocketToDisplay == SocketType.Left) sender.RefreshHalconScreen();
        }


        public GraphicsPackViewModel LeftGraphics
        {
            get { return (GraphicsPackViewModel) GetValue(LeftGraphicsProperty); }
            set { SetValue(LeftGraphicsProperty, value); }
        }
        
        

        public static readonly DependencyProperty RightGraphicsProperty = DependencyProperty.Register(
            "RightGraphics", typeof(GraphicsPackViewModel), typeof(HalconScreenView), new PropertyMetadata(default(GraphicsPackViewModel), OnRightResult2DChanged));

        private static void OnRightResult2DChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as HalconScreenView;
            if (sender.SocketToDisplay == SocketType.Right) sender.RefreshHalconScreen();
        }

        public GraphicsPackViewModel RightGraphics
        {
            get { return (GraphicsPackViewModel) GetValue(RightGraphicsProperty); }
            set { SetValue(RightGraphicsProperty, value); }
        }
        
        #endregion

        private HWindow _windowHandle;
        private void UpdateImageIndexList(GraphicsPackViewModel result2D)
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
            var result2D = SocketToDisplay == SocketType.Left ? LeftGraphics : RightGraphics;
            UpdateImageIndexList(result2D);
            var image = result2D.Images[ImageIndexToDisplay];
            var graphics = result2D.Graphics;
            _windowHandle.DispImage(image);
            _windowHandle.DispObj(graphics);
        }

    }
}