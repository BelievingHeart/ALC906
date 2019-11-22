
using System.Collections.Generic;
using HalconDotNet;

namespace Core.ImageProcessing
{
    public class MeasurementResult3D
    {
        public HImage CompositeImage { get; set; }

        public HObject CoordinateLiens { get; set; }

        public HObject Crosses { get; set; }

        public Dictionary<string, double> FaiResults { get; set; }

        public bool ItemExists { get; set; } = true;
    }
}