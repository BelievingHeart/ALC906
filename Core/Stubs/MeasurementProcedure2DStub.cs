using System.Collections.Generic;
using Core.ImageProcessing;
using HalconDotNet;

namespace Core.Stubs
{
    public class MeasurementProcedure2DStub : IMeasurementProcedure2D
    {
        public MeasurementResult2D Execute(List<HImage> images, Dictionary<string, FindLineParam> findLineParams)
        {
            return new MeasurementResult2D()
            { 
                FaiResults = new Dictionary<string, double>()
                {
                    ["2.1"] = 999,
                    ["2.2"] = 999
                }
            };
        }
    }
}