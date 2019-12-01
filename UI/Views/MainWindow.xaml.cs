using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Core.ViewModels.Application;
using Core.ViewModels.MainWindow;
using Core.ViewModels.Plc;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void CleanupApplication(object sender, EventArgs eventArgs)
        {
            //TODO: uncomment this line
            ApplicationViewModel.Cleanup();
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.InitHardWares();
            ApplicationViewModel.Instance.LoadFiles();
        }


        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void RestoreWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
        }
    }
}