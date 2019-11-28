using System;
using System.IO;
using Core.Constants;

namespace Core.Helpers
{
    public static class TimeHelper
    {
        public static string CurrentHour => DateTime.Now.ToString("HH");
        
    }
}