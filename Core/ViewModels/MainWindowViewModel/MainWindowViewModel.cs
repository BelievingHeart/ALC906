﻿using System.Windows.Input;
using Core.Commands;
using Core.Enums;

namespace Core.ViewModels.MainWindowViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Commands

        public ICommand SwitchHomePageCommand { get; set; }
        public ICommand SwitchCameraHostPageCommand { get; set; }
        public ICommand SwitchLineScanHostPageCommand { get; set; }
        public ICommand SwitchServerPageCommand { get; set; }

        #endregion

        #region Command Implemtations

        private void SwitchHomePage()
        {
            ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.Home;
            IoC.IoC.Logger.Log("Switched to home page");
        }
        
        private void SwitchCameraHostPage()
        {
            ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.CameraHostPage;
            IoC.IoC.Logger.Log("Switched to camera host page");

        }
        
        private void SwitchLineScanHostPage()
        {
            ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.LineScanHostPage;
            IoC.IoC.Logger.Log("Switched to line scan host page");

        }
        
        private void SwitchServerPage()
        {
            ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.ServerPage;
            IoC.IoC.Logger.Log("Switched to server page");

        }

        #endregion


        public MainWindowViewModel()
        {
            SwitchHomePageCommand = new SimpleCommand(o=>SwitchHomePage(), o=>ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage != ApplicationPageType.Home);
            SwitchCameraHostPageCommand = new SimpleCommand(o=>SwitchCameraHostPage(), o=>ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage != ApplicationPageType.CameraHostPage);
            SwitchLineScanHostPageCommand = new SimpleCommand(o=>SwitchLineScanHostPage(), o=>ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage != ApplicationPageType.LineScanHostPage);
            SwitchServerPageCommand = new SimpleCommand(o=>SwitchServerPage(), o=>ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage != ApplicationPageType.ServerPage);
        }
        
        
    }
}