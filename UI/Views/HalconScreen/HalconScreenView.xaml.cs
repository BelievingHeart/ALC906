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
            SocketToDisplay = CavityType.Cavity1;
            _windowHandle = HalconScreen.HalconWindow;
            _windowHandle.SetColored(3);
            _windowHandle.SetPart(0,0,-2,-2);
            _windowHandle.SetColor("green");
            _windowHandle.SetDraw("margin");
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
            SocketToDisplay = SocketToDisplay == CavityType.Cavity1 ? CavityType.Cavity2 : CavityType.Cavity1;
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
            "SocketToDisplay", typeof(CavityType), typeof(HalconScreenView), new PropertyMetadata(default(CavityType), OnSocketToDisplayChanged));

        private static void OnSocketToDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (HalconScreenView) d;
            sender.RefreshHalconScreen();
        }

        public CavityType SocketToDisplay
        {
            get { return (CavityType) GetValue(SocketToDisplayProperty); }
            set { SetValue(SocketToDisplayProperty, value); }
        }

        #region Results2D

        public static readonly DependencyProperty LeftGraphicsProperty = DependencyProperty.Register(
            "LeftGraphics", typeof(GraphicsPackViewModel), typeof(HalconScreenView), new PropertyMetadata(default(GraphicsPackViewModel), OnLeftResult2DChanged));

        private static void OnLeftResult2DChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as HalconScreenView;
            if (sender.SocketToDisplay == CavityType.Cavity1) sender.RefreshHalconScreen();
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
            if (sender.SocketToDisplay == CavityType.Cavity2) sender.RefreshHalconScreen();
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
            if (result2D.Images!=null)
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
            if (_windowHandle == null) return;
            _windowHandle.ClearWindow();
            var result2D = SocketToDisplay == CavityType.Cavity1 ? LeftGraphics : RightGraphics;
            UpdateImageIndexList(result2D);
            if(result2D.Images == null || result2D.Images.Count ==0) return;
            var image = result2D.Images[ImageIndexToDisplay];
            var graphics = result2D.Graphics;
            _windowHandle.DispImage(image);
            if(graphics!=null) _windowHandle.DispObj(graphics);
            var errorMessage = result2D.ErrorMessage;
            if(errorMessage != null) _windowHandle.DispText(errorMessage, "window", 20, 20, "red", "border_radius", 2);
        }

    }
}