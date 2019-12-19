using System;
using System.Globalization;
using WPFCommon.Converters;

namespace UI.Converters
{
    public class LogicNotConverter : ValueConverterBase<LogicNotConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool) value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}