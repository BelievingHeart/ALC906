using System.Collections.Generic;
using HalconDotNet;
using WPFCommon.ViewModels.Base;

namespace Core.ImageProcessing
{
    public class MeasurementResult2D : ViewModelBase
    {
        public Dictionary<string, double> FaiResults { get; set; }

        public HObject Graphics { get; set; }

        public List<HImage> Images { get; set; }
    }
}