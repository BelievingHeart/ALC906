using System;
using System.IO;
using WPFCommon.Helpers;

namespace Core.Constants
{
    public static class DirectoryConstants
    {
        public static string FaiConfigDir => Path.Combine(DirectoryHelper.ConfigDirectory, "Fai");
        public static string FindLineParamsConfigDir => Path.Combine(DirectoryHelper.ConfigDirectory, "FindLineParams");
        
 

        public static string CsvOutputDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ALC/CSV");
        
        private static string FaiConfigDir2D => Path.Combine(FaiConfigDir, "2D");
        private static string FaiConfigDir3D => Path.Combine(FaiConfigDir, "3D");
        
        public static string FaiConfigDir2DLeft => Path.Combine(FaiConfigDir2D, "Left");
        public static string FaiConfigDir2DRight => Path.Combine(FaiConfigDir2D, "Right");
        
        public static string FaiConfigDir3DLeft => Path.Combine(FaiConfigDir3D, "Left");
        public static string FaiConfigDir3DRight => Path.Combine(FaiConfigDir3D, "Right");

    }
}