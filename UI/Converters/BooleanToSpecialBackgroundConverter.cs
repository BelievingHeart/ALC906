using System;
using System.Globalization;
using System.Windows.Media;
using WPFCommon.Converters;

namespace UI.Converters
{
    public class BooleanToSpecialBackgroundConverter : ValueConverterBase<BooleanToSpecialBackgroundConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isSpecial = (bool) value;
            return isSpecial ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Gray);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}