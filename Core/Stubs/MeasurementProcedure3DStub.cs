using System;
using System.Collections.Generic;
using Core.ImageProcessing;
using Core.ViewModels.Application;
using HalconDotNet;

namespace Core.Stubs
{
    public class MeasurementProcedure3DStub : IMeasurementProcedure3D
    {
        public MeasurementResult3D Execute(List<HImage> images, HTuple shapeModel)
        {
            ApplicationViewModel.Instance.LogRoutine("Processed one 3D socket");
            
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