using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;

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
    }
}