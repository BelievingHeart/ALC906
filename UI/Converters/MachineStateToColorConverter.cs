using System;
using System.Globalization;
using System.Windows.Media;
using PLCCommunication.Core.Enums;

namespace UI.Converters
{
    public class MachineStateToColorConverter : ValueConverterBase<MachineStateToColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (MachineState) value;
            switch (state)
            {
                case MachineState.Disconnect:
                    return new SolidColorBrush(Colors.Salmon);
                case MachineState.Idle:
                    return new SolidColorBrush(Colors.Chartreuse);
                case MachineState.Pausing:
                    return new SolidColorBrush(Colors.Aqua);
                case MachineState.Resetting:
                    return new SolidColorBrush(Colors.Goldenrod);
                case MachineState.Running:
                    return new SolidColorBrush(Colors.Orchid);
                case MachineState.Stopping:
                    return new SolidColorBrush(Colors.Teal);
            }

            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}