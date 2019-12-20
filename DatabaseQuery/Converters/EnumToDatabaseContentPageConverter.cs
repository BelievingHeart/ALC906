using System;
using System.Globalization;
using System.Windows.Controls;
using Core.Enums;
using DatabaseQuery.Views;
using WPFCommon.Converters;

namespace DatabaseQuery.Converters
{
    public class EnumToDatabaseContentPageConverter : ValueConverterBase<EnumToDatabaseContentPageConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var newValue = (DatabaseContentPageType) value;
            return newValue == DatabaseContentPageType.SettingPage ? new SettingView() : new TableView() as UserControl;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}