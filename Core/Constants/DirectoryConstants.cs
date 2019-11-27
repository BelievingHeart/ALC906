using System;
using System.IO;
using WPFCommon.Helpers;

namespace Core.Constants
{
    public static class DirectoryConstants
    {
        public static string FaiConfigDir => Path.Combine(DirectoryHelper.ConfigDirectory, "Fai");
        public static string FindLineParamsConfigDir => Path.Combine(DirectoryHelper.ConfigDirectory, "FindLineParams");
        
 

        /// <summary>
        /// Base dir to output various records
        /// </summary>
        public static string OutputDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ALC");
        public static string CsvOutputDir => Path.Combine(OutputDir, "CSV");

        /// <summary>
        /// Directory that stores general records
        /// </summary>
        public static string ProductionLineRecordDir => Path.Combine(OutputDir, "General");

        
        private static string FaiConfigDir2D => Path.Combine(FaiConfigDir, "2D");
        private static string FaiConfigDir3D => Path.Combine(FaiConfigDir, "3D");
        
        public static string FaiConfigDir2DLeft => Path.Combine(FaiConfigDir2D, "Left");
        public static string FaiConfigDir2DRight => Path.Combine(FaiConfigDir2D, "Right");
        
        public static string FaiConfigDir3DLeft => Path.Combine(FaiConfigDir3D, "Left");
        public static string FaiConfigDir3DRight => Path.Combine(FaiConfigDir3D, "Right");
        
        //TODO: remove the following dirs
        public static string ImageDir2D => Path.Combine(Directory.GetCurrentDirectory(), "2D");
        public static string ImageDir3D => Path.Combine(Directory.GetCurrentDirectory(), "3D");
        
    }
}