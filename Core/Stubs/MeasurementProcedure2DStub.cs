using System;
using System.Collections.Generic;
using Core.ImageProcessing;
using HalconDotNet;

namespace Core.Stubs
{
    public class MeasurementProcedure2DStub : IMeasurementProcedure2D
    {
        public MeasurementResult2D Execute(List<HImage> images, Dictionary<string, FindLineParam> findLineParams)
        {
            //TODO: remove the following lines
            foreach (var image in images)
            {
                var filePath = "D:\\share\\3DImages\\Tests/" + DateTime.Now.ToString("hhmmssff") + ".bmp";
                image.WriteImage("bmp", 0, filePath);
            }
            
            
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