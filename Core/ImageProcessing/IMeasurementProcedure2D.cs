using System.Collections.Generic;
using HalconDotNet;

namespace Core.ImageProcessing
{
    public interface IMeasurementProcedure2D
    {
        MeasurementResult2D Execute(List<HImage> images, Dictionary<string, FindLineParam> findLineParams);
    }
}