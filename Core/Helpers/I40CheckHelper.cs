﻿using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using Core.Enums;
using Core.ImageProcessing;
using CYG906ALC.ALG;

namespace Core.Helpers
{
    /// <summary>
    /// Helper class to convert types from image processing units' modules
    /// </summary>
    public static class I40CheckHelper
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

        public static MeasurementResult2D GetMeasurementResult(this I40Check procedure, SocketType socketType)
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
            
            return new MeasurementResult2D(){FaiResults = output};
        }
    }
}