using System;
using System.Globalization;
using PLCCommunication.Core.Enums;
using WPFCommon.Converters;

namespace UI.Converters
{
    public class MachineStateTranslationConverter : ValueConverterBase<MachineStateTranslationConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumValue = (MachineState) value;
            switch (enumValue)
            {
                case MachineState.Disconnect:
                    return "连接断开";
                case MachineState.Idle:
                    return "准备";
                case MachineState.Pausing:
                    return "暂停中";
                case MachineState.Resetting:
                    return "正在复位";
                case MachineState.Running:
                    return "自动运行中";
                case MachineState.Stopping:
                    return "正在停止";
            }

            throw new NotSupportedException($"Can not find such machine state : {enumValue}");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}