using Core.IoC.Interface;
using Core.ViewModels.ApplicationViewModel;

namespace UI.DataAccess
{
    public class LoggingServiceProvider : ILoggingServiceProvider
    {
        public void Log(string message)
        {
            ApplicationViewModel.Instance.MessageQueue.Enqueue(message);
        }
    }
}