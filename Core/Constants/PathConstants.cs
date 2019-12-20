using System.IO;
using WPFCommon.Helpers;

namespace Core.Constants
{
    public static class PathConstants
    {
        public static string ShapeModelPath2D
        {
            get { return Path.Combine(DirectoryHelper.ConfigDirectory, "ShapeModel/2D"); }
        }

        public static string ShapeModelPath3D
        {
            get { return Path.Combine(DirectoryHelper.ConfigDirectory, "ShapeModel/3D"); }
        }
    }
}