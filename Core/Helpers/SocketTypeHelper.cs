using Core.Enums;

namespace Core.Helpers
{
    public static class SocketTypeHelper
    {
        public static int ToChusIndex(this SocketType socketType)
        {
            switch (socketType)
            {
                case SocketType.Left:
                    return 0;
                case SocketType.Right:
                    return 1;
            }

            return -1;
        }
    }
}