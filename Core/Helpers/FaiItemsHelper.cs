using System.Collections.Generic;
using Core.ViewModels.Fai;

namespace Core.Helpers
{
    public static class FaiItemsHelper
    {

        public static List<FaiItem> ConcatNew(this IEnumerable<FaiItem> me, IEnumerable<FaiItem> other)
        {
            var output = new List<FaiItem>();
            if(me!=null) output.AddRange(me);
            if(other!=null) output.AddRange(other);
            return output;
        }
    }
}