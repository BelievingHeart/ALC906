using System.Collections.Generic;
using Core.Enums;
using Core.ViewModels.Fai;
using Core.ViewModels.Summary;

namespace Core.Helpers
{
    public static class SummaryHelper
    {
        public static void UpdateSummary(this SummaryViewModel summary,
            params ProductLevel[] productLevels)
        {
            foreach (var productLevel in productLevels)
            {
                switch (productLevel)
                {
                    case ProductLevel.Empty:
                        summary.EmptyCount++;
                        break;
                    case ProductLevel.OK:
                        summary.OkCount++;
                        break;
                    case ProductLevel.Ng2:
                        summary.Ng2Count++;
                        break;
//                    case ProductLevel.Ng3:
//                        summary.Ng3Count++;
//                        break;
                    case ProductLevel.Ng4:
                        summary.Ng4Count++;
                        break;
                    case ProductLevel.Ng5:
                        summary.Ng5Count++;
                        break;
                }

            }
        }

        public static void UpdateYieldCollection(this SummaryViewModel summaryViewModel, params IEnumerable<FaiItem>[] faiItems)
        {
            foreach (var faiItem in faiItems)
            {
                if(faiItem!=null) summaryViewModel.FaiYieldCollectionViewModel.Update(faiItem);
            }
        }
    }
}