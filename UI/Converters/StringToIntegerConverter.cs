using System;
using System.Globalization;
using System.Windows.Controls;
using WPFCommon.Converters;

namespace UI.Converters
{
    public class StringToIntegerConverter : ValueConverterBase<StringToIntegerConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            var item = (ComboBoxItem) value;
            var count = int.Parse((string) item.Content);
            return count;
        }
    }
}