using System;
using System.Collections.Generic;
using Core.ImageProcessing;
using Core.ViewModels.Application;
using HalconDotNet;

namespace Core.Stubs
{
    public class MeasurementProcedure2DStub : IMeasurementProcedure2D
    {
        public MeasurementResult2D Execute(List<HImage> images, Dictionary<string, FindLineParam> findLineParams)
        {
            ApplicationViewModel.Instance.LogRoutine("Processed one 2D socket");
            
            return new MeasurementResult2D()
            { 
                FaiResults = new Dictionary<string, double>()
                {
                    ["1-1A"] = 999,
                    ["1-1B"] = 999
                }
            };
        }
    }
}