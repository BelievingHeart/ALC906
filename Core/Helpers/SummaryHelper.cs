using Core.Enums;
using Core.ViewModels.Summary;

namespace Core.Helpers
{
    public static class SummaryHelper
    {
        public static void Update(this SummaryViewModel summary,
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
                    case ProductLevel.Ng3:
                        summary.Ng3Count++;
                        break;
                    case ProductLevel.Ng4:
                        summary.Ng4Count++;
                        break;
                    case ProductLevel.Ng5:
                        summary.Ng5Count++;
                        break;
                }

                if (productLevel == ProductLevel.Empty) continue;
            }
        }
    }
}