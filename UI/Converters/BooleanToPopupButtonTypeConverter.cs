using System;
using System.Globalization;
using System.Windows.Controls;
using UI.Views.Popup;

namespace UI.Converters
{
    public class BooleanToPopupButtonTypeConverter : ValueConverterBase<BooleanToPopupButtonTypeConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isSpecial = (bool) value;
            return isSpecial ? new PopupButtonsSpecial() : new PopupButtonsNormal() as UserControl;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}