using Core.ViewModels.Application;
using LJX8000.Core.IoC.Interface;

namespace Core.IoC.Loggers
{
    /// <summary>
    /// Log messages that bubble up from LJX8000.Core
    /// </summary>
    public class LaserControllerLibLogger : ILJX8000LibLogger
    {
        public void LogThreadSafe(string message)
        {
            Logger.LogRoutineMessageAsync(message);
        }
    }
}