using System.Collections.Generic;
using Core.ViewModels.Results;
using HalconDotNet;

namespace Core.ImageProcessing
{
    public interface IMeasurementProcedure2D
    {
        GraphicsPackViewModel Execute(List<HImage> images, Dictionary<string, FindLineParam> findLineParams);
    }
}