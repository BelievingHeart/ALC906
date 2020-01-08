using Core.Enums;

namespace Core.Helpers
{
    public static class ProductLevelHelper
    {
        public static string GetResultText(this ProductLevel productLevel)
        {
            if (productLevel == ProductLevel.OK) return "OK";
            if (productLevel == ProductLevel.Empty) return "Empty";
            if (productLevel == ProductLevel.Ng5) return "Error";
            if (productLevel == ProductLevel.Ng3) return "WrongProductType";
            return "NG";
        }
    }
}