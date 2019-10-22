using Core.Enums;

namespace Core.ViewModels.ApplicationViewModel
{
    
    /// <summary>
    /// Holds the global states of the application
    /// </summary>
    public class ApplicationViewModel : ViewModelBase
    {
        /// <summary>
        /// Static instance for xaml to bind to 
        /// </summary>
        public static ApplicationViewModel Instance => _instance;
        private static ApplicationViewModel _instance = new ApplicationViewModel()
        {
            CurrentApplicationPage = ApplicationPageType.Home
        };


        /// <summary>
        /// Current application page
        /// </summary>
        public ApplicationPageType CurrentApplicationPage { get; set; }
        
        
    }
}