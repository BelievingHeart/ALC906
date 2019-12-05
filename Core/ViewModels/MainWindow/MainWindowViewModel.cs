using System.Windows.Input;
using Core.Enums;
using Core.ViewModels.Application;
using WPFCommon.Commands;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.MainWindow
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Commands

        public ICommand SwitchHomePageCommand { get; set; }
        public ICommand SwitchCameraHostPageCommand { get; set; }
        public ICommand SwitchLineScanHostPageCommand { get; set; }
        public ICommand SwitchServerPageCommand { get; set; }

        public ICommand SwitchSettingsPageCommand { get; }

        #endregion

        #region Command Implemtations

        private void SwitchHomePage()
        {
            ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.Home;
        }

        private void SwitchCameraHostPage()
        {
            ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.CameraHostPage;
        }

        private void SwitchLineScanHostPage()
        {
            ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.LineScanHostPage;
        }

        private void SwitchServerPage()
        {
            ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.ServerPage;
        }
        
        private void SwitchSettingsPage()
        {
            ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.SettingsPage;
        }

        #endregion


        public MainWindowViewModel()
        {
            SwitchHomePageCommand = new SimpleCommand(o => SwitchHomePage(),
                o => ApplicationViewModel.Instance.CurrentApplicationPage != ApplicationPageType.Home);
            SwitchCameraHostPageCommand = new SimpleCommand(o => SwitchCameraHostPage(),
                o => ApplicationViewModel.Instance.CurrentApplicationPage != ApplicationPageType.CameraHostPage);
            SwitchLineScanHostPageCommand = new SimpleCommand(o => SwitchLineScanHostPage(),
                o => ApplicationViewModel.Instance.CurrentApplicationPage != ApplicationPageType.LineScanHostPage);
            SwitchServerPageCommand = new SimpleCommand(o => SwitchServerPage(),
                o => ApplicationViewModel.Instance.CurrentApplicationPage != ApplicationPageType.ServerPage);
            SwitchSettingsPageCommand = new SimpleCommand(o => SwitchSettingsPage(),
                o => ApplicationViewModel.Instance.CurrentApplicationPage != ApplicationPageType.SettingsPage);

        }

      
    }
}