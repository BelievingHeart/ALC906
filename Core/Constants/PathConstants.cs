using System.IO;
using WPFCommon.Helpers;

namespace Core.Constants
{
    public static class PathConstants
    {
        public static string ShapeModelPath2D => Path.Combine(DirectoryHelper.ConfigDirectory, "ShapeModel/2D");
        public static string ShapeModelPath3D => Path.Combine(DirectoryHelper.ConfigDirectory, "ShapeModel/3D");
    }
}