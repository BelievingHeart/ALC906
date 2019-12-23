using System;
using System.Collections.Generic;
using System.Globalization;
using Core.Models;

namespace Core.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime? ToDate(this string dateTimeStr, params string[] dateFmt)
        {
            // example: var dt = "2011-03-21 13:26".ToDate(new string[]{"yyyy-MM-dd HH:mm", 
            //                                                  "M/d/yyyy h:mm:ss tt"});
            // or simpler: 
            // var dt = "2011-03-21 13:26".ToDate("yyyy-MM-dd HH:mm", "M/d/yyyy h:mm:ss tt");
            const DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces;
            if (dateFmt == null)
            {
                var dateInfo = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat;
                dateFmt=dateInfo.GetAllDateTimePatterns();
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
                style, out dt) ? dt : null as DateTime?;
            return result;
        }
        
        public static List<DateTimePair> GetDateTimePairs(DateTime fromDate, DateTime toDate, int parts) {
            TimeSpan bigInterval = toDate - fromDate;
            TimeSpan smallInterval = new TimeSpan(bigInterval.Ticks / parts);
            List<DateTimePair> returnValue = new List<DateTimePair>();
            DateTime temp = fromDate;

            while (temp < toDate) {
                DateTime currentFromDate = temp;
                DateTime currentToDate = temp + smallInterval;
                
                // Your logic

                temp = temp + smallInterval;
                returnValue.Add(new DateTimePair() { FromDateTime = currentFromDate, ToDateTime = currentToDate });
            }

            return returnValue;
        }
    }
}