using System;
using System.Collections.Generic;
using System.Globalization;
using Core.Models;

namespace Core.Helpers
{
    public static class DateTimeHelper
    {


        public static List<DateTimePair> GetDateTimePairs(DateTime fromDate, DateTime toDate, int parts)
        {
            TimeSpan bigInterval = toDate - fromDate;
            TimeSpan smallInterval = new TimeSpan(bigInterval.Ticks / parts);
            List<DateTimePair> returnValue = new List<DateTimePair>();
            DateTime temp = fromDate;

            while (temp < toDate)
            {
                DateTime currentFromDate = temp;
                DateTime currentToDate = temp + smallInterval;

                // Your logic

                temp = temp + smallInterval;
                returnValue.Add(new DateTimePair() {FromDateTime = currentFromDate, ToDateTime = currentToDate});
            }

            return returnValue;
        }
    }
}