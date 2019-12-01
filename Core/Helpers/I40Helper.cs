using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using Core.Enums;
using Core.ImageProcessing;
using Core.ViewModels.Results;
using CYG906ALC.ALG;
using HalconDotNet;
using I40_3D_Test;

namespace Core.Helpers
{
    /// <summary>
    /// Helper class to convert types from image processing units' modules
    /// </summary>
    public static class I40Helper
    {
        public static List<List<string>> ToStringMatrix(this Dictionary<string, List<string>> dictionary)
        {
            return dictionary.Values.ToList();
        }

        public static Dictionary<string, List<string>> ToDict(this List<List<string>> stringMatrix)
        {
            var output = new Dictionary<string, List<string>>();
            foreach (var list in stringMatrix)
            {
                output[list[0]] = list;
            }

            return output;
        }

        public static Dictionary<string, double> GetFaiDict(this I40Check procedure, SocketType socketType)
        {
            var socketIndex = (int) socketType;
            var numFaiItems = procedure.resultNum;
            var startIndex = socketIndex * numFaiItems;
            var endIndex = startIndex + numFaiItems;
            var output = new Dictionary<string, double>();
            for (int index = startIndex; index < endIndex; index++)
            {
                var result = procedure.myResult[index];
                output[result.nameStr] = result.measureValue;
            }

            return output;
        }

        public static GraphicsPackViewModel GetResultAndGraphics(this I40Check procedure, SocketType socketType, List<HImage> images)
        {
            var result = new GraphicsPackViewModel {Images = images};
            result.Graphics = procedure.OnGetCheckValue(images, (int) socketType, 0);
            result.FaiResults = procedure.GetFaiDict(socketType);

            return result;
        }

        public static GraphicsPackViewModel GetGraphics(this MeasurementResult3D result3D)
        {
            var output = new GraphicsPackViewModel();
            output.Graphics = result3D.CoordinateLinesAndCrosses;
            output.Images = result3D.CompositeImage;
            output.ErrorMessage = result3D.ErrorMessage;
            output.ItemExists = result3D.ItemExists;
            output.FaiResults = result3D.FaiResults;
            return output;
        }
    }
}