using System;
using System.Globalization;
using Core.Enums;

namespace UI.Converters
{
    public class ProductTypeToIndexConverter : ValueConverterBase<ProductTypeToIndexConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var productType = (ProductType) value;
            return (int) productType;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var index = (int) value;
            return (ProductType) index;
        }
    }
}