using System;

namespace Core.Helpers
{
    public static class TimeHelper
    {
        public static string CurrentHour
        {
            get { return DateTime.Now.ToString("HH"); }
        }
    }
}