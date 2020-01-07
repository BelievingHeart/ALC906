using System;
using System.Globalization;
using WPFCommon.Converters;

namespace UI.Converters
{
    public class DateTimeFormatConverter : ValueConverterBase<DateTimeFormatConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = (DateTime) value;
            return dateTime.ToString("t");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}