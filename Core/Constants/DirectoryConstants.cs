using System;
using System.IO;
using WPFCommon.Helpers;

namespace Core.Constants
{
    public static class DirectoryConstants
    {
        public static string FaiConfigDir => Path.Combine(DirectoryHelper.ConfigDirectory, "Fai");
        public static string Config2DDir => Path.Combine(Directory.GetCurrentDirectory(), "2DConfigs");
        

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
        public static string SummaryDirToday => Path.Combine(ProductionLineRecordDir, DateTime.Now.ToString("MM-dd"));
        
        public static string FaiNamesDir => Path.Combine(FaiConfigDir, "FaiNames");
        
        

        
        //TODO: remove the following dirs
        public static string ImageDirRight => Path.Combine(Directory.GetCurrentDirectory(), "Cavity2");
        public static string ImageDirLeft => Path.Combine(Directory.GetCurrentDirectory(), "Cavity1");
        
        
        
    }
}