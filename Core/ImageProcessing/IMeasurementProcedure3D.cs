using System.Collections.Generic;
using HalconDotNet;

namespace Core.ImageProcessing
{
    public interface IMeasurementProcedure3D
    {
        MeasurementResult3D Execute(List<HImage> images, HTuple shapeModel);
    }
}