using System.Collections.Generic;
using System.Linq;
using Core.ImageProcessing;

namespace Core.Helpers
{
    public static class FindLineParamHelper
    {
        public static Dictionary<string, FindLineParam> ToDict(this IEnumerable<FindLineParam> findLineParams)
        {
            return findLineParams.ToDictionary(param => param.Name, param => param);
        }
    }
}