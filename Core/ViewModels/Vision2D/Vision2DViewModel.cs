using Core.ImageProcessing;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Vision2D
{
    public class Vision2DViewModel : ViewModelBase
    {
        public IMeasurementProcedure2D MeasurementProcedure2D { get; set; }

    }
}