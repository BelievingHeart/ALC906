using System;
using System.Globalization;
using System.Windows.Controls;
using WPFCommon.Converters;

namespace DatabaseQuery.Converters
{
    public class IntegerToComboBoxItemConverter : ValueConverterBase<IntegerToComboBoxItemConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (ComboBoxItem) value;
            return item != null ? int.Parse((string) item.Content) : 20;
        }
    }
}