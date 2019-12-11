using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
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

        public static Dictionary<string, double> GetFaiDict(this I40Check procedure, int chusIndex)
        {
            var numFaiItems = procedure.resultNum;
            var startIndex = chusIndex * numFaiItems;
            var numValidFai = I40Check.YouXiaoFAINum;
            var endIndex = startIndex + numValidFai;
            var output = new Dictionary<string, double>();

            for (int index = startIndex; index < endIndex; index++)
            {
                var result = procedure.myResult[index];
                output[result.nameStr] = FloorDouble(result.measureValue, 4);
            }

            return output;
        }

        public static GraphicsPackViewModel Execute(this I40Check procedure, int chusIndex, List<HImage> images)
        {
            var result = new GraphicsPackViewModel
            {
                Images = images,
                Graphics = procedure.OnGetCheckValue(images, chusIndex, 0),
                FaiResults = procedure.GetFaiDict(chusIndex)
            };

            return result;
        }

        public static GraphicsPackViewModel GetGraphics(this MeasurementResult3D result3D)
        {
            var output = new GraphicsPackViewModel();
            output.Graphics = result3D.CoordinateLinesAndCrosses;
            output.Images = result3D.CompositeImage;
            output.ErrorMessage = result3D.ErrorMessage;
            output.ItemExists = result3D.ItemExists;
            var dict = new Dictionary<string, double>();
            if (result3D.ItemExists)
            {
                foreach (var key in result3D.FaiResults.Keys)
                {
                    dict[key] = FloorDouble(result3D.FaiResults[key], 4);
                }
            }

            output.FaiResults = dict;
            return output;
        }
        
        
        private static HImage HobjectToHimage(HObject hobject)
        {
            HTuple pointer, type, width, height;
            HImage output = new HImage();
            HOperatorSet.GetImagePointer1(hobject, out pointer, out type, out width, out height);
            output.GenImage1(type, width, height, pointer);
            
            return output;
        }

        /// <summary>
        /// Floor double number to the specified precision
        /// </summary>
        /// <param name="value">value to floor</param>
        /// <param name="numDecimalToPreserve"></param>
        /// <returns></returns>
        public static double FloorDouble(double value, int numDecimalToPreserve)
        {
            var mulFactor = 1;
            for (int i = 0; i < numDecimalToPreserve; i++)
            {
                mulFactor *= 10;
            }

            var temp = (int) (value * mulFactor);

            return temp / (double) mulFactor;
        }
    }
}