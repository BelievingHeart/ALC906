﻿using Core.ViewModels.Application;
using HKCameraDev.Core.IoC.Interface;

namespace Core.IoC.Loggers
{
    /// <summary>
    /// Log messages that bubble up from HKCameraDev.Core
    /// </summary>
    public class CameraMessageLogger : IHKCameraLibLogger
    {
        public void Log(string message)
        {
            ApplicationViewModel.Instance?.LogRoutine(message);
        }
    }
}