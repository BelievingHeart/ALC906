using System;
using System.Globalization;
using WPFCommon.Converters;

namespace DatabaseQuery.Converters
{
    public class Plus1Converter : ValueConverterBase<Plus1Converter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var index = (int) value;
            return index + 1;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}