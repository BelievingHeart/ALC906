
using System.Collections.Generic;
using HalconDotNet;

namespace Core.ImageProcessing
{
    public class MeasurementResult3D
    {
        /// <summary>
        /// 合成图像
        /// </summary>
        public HImage CompositeImage { get; set; }

        /// <summary>
        /// 拟合的基准线
        /// </summary>
        public HObject CoordinateLines { get; set; }

        /// <summary>
        /// 找线ROI
        /// </summary>
        public HObject FindLineRectangles { get; set; }

        /// <summary>
        /// 绿线的REGION
        /// </summary>
        public HObject RegionB { get; set; }
        
        /// <summary>
        /// 点位
        /// </summary>
        public HObject Crosses { get; set; }

        /// <summary>
        /// FAI结果
        /// </summary>
        public Dictionary<string, double> FaiResults { get; set; }

        /// <summary>
        /// 是否有料
        /// </summary>
        public bool ItemExists { get; set; } = true;
    }
}