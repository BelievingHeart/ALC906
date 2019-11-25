using Core.ViewModels.Application;
using HKCameraDev.Core.IoC.Interface;

namespace Core.IoC.Loggers
{
    public class CameraMessageLogger : IUILogger
    {
        public void Log(string message)
        {
            ApplicationViewModel.Instance?.LogRoutine(message);
        }
    }
}