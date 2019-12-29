using System;
using System.Globalization;
using Core.Enums;
using DatabaseQuery.Views;
using DatabaseQuery.Views.Dialogs;
using WPFCommon.Converters;

namespace DatabaseQuery.Converters
{
    public class EnumToDatabaseViewDialogConverter : ValueConverterBase<EnumToDatabaseViewDialogConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumValue = (DatabaseViewDialogType) value;
            switch (enumValue)
            {
                case DatabaseViewDialogType.DeleteDialog:
                    return new DeleteCollectionsDialog();
                case DatabaseViewDialogType.SaveDialog:
                    return new SaveCollectionsDialog();
                case DatabaseViewDialogType.LineChartDialog:
                    return new LineChartDialog();
                case DatabaseViewDialogType.PieChartDialog:
                    return new PieChartDialog();
                case DatabaseViewDialogType.WaitingDialog:
                    return new WaitingDialog();
                case DatabaseViewDialogType.LoginDialog:
                    return new LoginView();
            }

            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}