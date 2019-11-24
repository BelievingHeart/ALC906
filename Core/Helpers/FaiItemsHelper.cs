using System.Collections.Generic;
using Core.ViewModels.Fai;

namespace Core.Helpers
{
    public static class FaiItemsHelper
    {
        /// <summary>
        /// Set all fai items' value to 999
        /// </summary>
        /// <param name="faiItems"></param>
        public static void UnsetValues(this IEnumerable<FaiItem> faiItems)
        {
            foreach (var item in faiItems)
            {
                item.Value = 999;
            }
        }

        public static List<FaiItem> ConcatNew(this IEnumerable<FaiItem> me, IEnumerable<FaiItem> other)
        {
            var output = new List<FaiItem>();
            output.AddRange(me);
            output.AddRange(other);
            return output;
        }
    }
}