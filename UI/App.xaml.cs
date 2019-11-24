using System.Windows;
using Core.IoC;
using Core.IoC.Interface;
using Core.ViewModels.Application;
using UI.DataAccess;
using UI.Views;

namespace UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// Set up IoC and manage window start up myself
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // IoC setup
            IoC.Kernel.Bind<ILoggingServiceProvider>().ToConstant(new LoggingServiceProvider());
            IoC.Setup();
            ApplicationViewModel.Init();
            
            // Start main window
            var window = new MainWindow();
            Current.MainWindow = window;
            window.Show();
        }
    }
}