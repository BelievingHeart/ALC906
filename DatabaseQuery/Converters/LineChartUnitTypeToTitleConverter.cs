using System;
using System.Globalization;
using Core.Enums;
using WPFCommon.Converters;

namespace DatabaseQuery.Converters
{
    public class LineChartUnitTypeToTitleConverter : ValueConverterBase<LineChartUnitTypeToTitleConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumValue = (LineChartUnitType) value;
            return $"Units per {enumValue.ToString()}";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}