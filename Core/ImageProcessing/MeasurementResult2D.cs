using System.Collections.Generic;
using HalconDotNet;
using WPFCommon.ViewModels.Base;

namespace Core.ImageProcessing
{
    public class MeasurementResult2D : ViewModelBase
    {
        public Dictionary<string, double> FaiResults { get; set; }

        public HObject PointsIgnore { get; set; }

        public HObject PointsUsed { get; set; }

        public HObject FitLines { get; set; }

        public HObject RoiRectangles { get; set; }

        public HObject Contours { get; set; }

        public List<HImage> Images { get; set; }
    }
}