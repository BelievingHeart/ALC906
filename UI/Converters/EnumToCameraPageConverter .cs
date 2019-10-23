using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core.Enums;
using Core.ViewModels.CameraViewModel;
using UI.Views.CameraView;

namespace UI.Converters
{
    //TODO: finish this class
    public class EnumToCameraPageConverter : ValueConverterBase<EnumToCameraPageConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var pageEnum = (CameraPageType) value;
                return RetrievePage(pageEnum);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static CameraView RetrievePage(CameraPageType pageEnum)
        {
            CameraView output;
            // Try get the first camera page with specific precedure
            try
            {
                var cameraViewModel = pageEnum == CameraPageType.TopCamera
                    ? CameraViewModels.First(model => model.MeasurementProcedure.Name == "TopCamera")
                    : CameraViewModels.First(model => model.MeasurementProcedure.Name == "BottomCamera");
                output = new CameraView()
                {
                    DataContext = cameraViewModel
                };
            }
            // If the list not contain an instance of such type
            // Add one and return it
            catch (InvalidOperationException e)
            {
           
                throw new NotImplementedException("Define procedure type ... ");
            }


            return output;
        }

        private static List<CameraViewModel> CameraViewModels { get; } = new List<CameraViewModel>();
    }
}