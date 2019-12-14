using Core.Enums;

namespace Core.Helpers
{
    public static class SocketTypeHelper
    {
        public static int ToChusIndex(this CavityType socketType)
        {
            switch (socketType)
            {
                case CavityType.Cavity1:
                    return 0;
                case CavityType.Cavity2:
                    return 1;
            }

            return -1;
        }
    }
}