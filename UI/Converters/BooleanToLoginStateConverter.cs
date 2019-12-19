using System;
using System.Globalization;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using WPFCommon.Converters;

namespace UI.Converters
{
    public class BooleanToLoginStateConverter : ValueConverterBase<BooleanToLoginStateConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var loggedIn = (bool) value;
            var size = 25;
            return loggedIn
                ? new PackIcon() {Kind = PackIconKind.LockOpen, Foreground = Brushes.Green, Width = size, Height = size, ToolTip = "Logged in"}
                : new PackIcon() {Kind = PackIconKind.Lock, Foreground = Brushes.Red, Width = size, Height = size, ToolTip = "Logged out"};
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}