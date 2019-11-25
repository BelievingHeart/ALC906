using System;
using System.Collections.Generic;
using Core.ImageProcessing;
using HalconDotNet;

namespace Core.Stubs
{
    public class MeasurementProcedure3DStub : IMeasurementProcedure3D
    {
        public MeasurementResult3D Execute(List<HImage> images, HTuple shapeModel)
        {
            //TODO: remove the following lines
            foreach (var image in images)
            {
                var filePath = "D:\\share\\3DImages\\Tests/" + DateTime.Now.ToString("hhmmssff") + ".tif";
                image.WriteImage("tiff", 0, filePath);
            }
            
            return new MeasurementResult3D()
            {
                CompositeImage = images[0],
                FaiResults = new Dictionary<string, double>()
                {
                    ["16.1"] = 999,
                    ["16.2"] = 999
                }
            };
        }
    }
}