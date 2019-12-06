using Core.Enums;

namespace Core.Helpers
{
    public static class SocketTypeHelper
    {
        public static int ToChusIndex(this SocketType socketType)
        {
            switch (socketType)
            {
                case SocketType.Cavity1:
                    return 0;
                case SocketType.Cavity2:
                    return 1;
            }

            return -1;
        }
    }
}