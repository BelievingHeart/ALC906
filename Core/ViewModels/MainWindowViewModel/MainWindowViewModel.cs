using System.Windows.Input;
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
        }
        
        private void SwitchCameraHostPage()
        {
            ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.CameraHostPage;
        }
        
        private void SwitchLineScanHostPage()
        {
            ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.LineScanHostPage;
        }
        
        private void SwitchServerPage()
        {
            ApplicationViewModel.ApplicationViewModel.Instance.CurrentApplicationPage = ApplicationPageType.ServerPage;
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