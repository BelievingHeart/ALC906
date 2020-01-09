using System;
using System.Globalization;
using WPFCommon.Converters;

namespace UI.Converters
{
    public class DayHourMinuteToStringConverter : ValueConverterBase<DayHourMinuteToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = (DateTime) value;
            return dateTime.ToString("MMdd/HH:mm");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}