using System.IO;
using WPFCommon.Helpers;

namespace Core.Constants
{
    public static class DirectoryConstants
    {
        public static string FaiConfigDir => Path.Combine(DirectoryHelper.ConfigDirectory, "Fai");
        public static string FindLineParamsConfigDir => Path.Combine(DirectoryHelper.ConfigDirectory, "FindLineParams");
        public static string LeftFaiConfigDir => Path.Combine(FaiConfigDir, "Left");
        public static string RightFaiConfigDir => Path.Combine(FaiConfigDir, "Right");
    }
}