using System;
using System.Globalization;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace UI.Converters
{
    public class BooleanToLoginStateConverter : ValueConverterBase<BooleanToLoginStateConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var loginedIn = (bool) value;
            var size = 25;
            return loginedIn
                ? new PackIcon() {Kind = PackIconKind.LockOpen, Foreground = Brushes.Green, Width = size, Height = size}
                : new PackIcon() {Kind = PackIconKind.Lock, Foreground = Brushes.Red, Width = size, Height = size};
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}