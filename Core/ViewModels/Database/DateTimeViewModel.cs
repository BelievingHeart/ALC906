using System;
using System.Globalization;
using Core.Helpers;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Database
{
    public class DateTimeViewModel : ViewModelBase
    {
        public int Year { get; set; }= int.Parse(DateTime.Now.ToString("yyyy"));
        public int Month { get; set; } = int.Parse(DateTime.Now.ToString("MM"));
        public int Day { get; set; } = int.Parse(DateTime.Now.ToString("dd"));
        public int Hour { get; set; } = int.Parse(DateTime.Now.ToString("HH"));
        public int Minute { get; set; } = 0;
        
        public DateTime? ToDateTime()
        {
            var dateText = $"{Year}-{Month:D2}-{Day:D2} {Hour:D2}:{Minute:D2}";
            var date = ToDate(dateText,"yyyy-MM-dd HH:mm");
            return date;
        }
        
        
        
        private static DateTime? ToDate(string dateTimeStr, params string[] dateFmt)
        {
            // example: var dt = "2011-03-21 13:26".ToDate(new string[]{"yyyy-MM-dd HH:mm", 
            //                                                  "M/d/yyyy h:mm:ss tt"});
            // or simpler: 
            // var dt = "2011-03-21 13:26".ToDate("yyyy-MM-dd HH:mm", "M/d/yyyy h:mm:ss tt");
            const DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces;
            if (dateFmt == null)
            {
                var dateInfo = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat;
                dateFmt = dateInfo.GetAllDateTimePatterns();
            }

            // Commented out below because it can be done shorter as shown below.
            // For older C# versions (older than C#7) you need it like that:
            // DateTime? result = null;
            // DateTime dt;
            // if (DateTime.TryParseExact(dateTimeStr, dateFmt,
            //    CultureInfo.InvariantCulture, style, out dt)) result = dt;
            // In C#7 and above, we can simply write:
            System.DateTime dt;
            var result = DateTime.TryParseExact(dateTimeStr, dateFmt, CultureInfo.InvariantCulture,
                style, out dt)
                ? dt
                : null as DateTime?;
            return result;
        }
    }
}