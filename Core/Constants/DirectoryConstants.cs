using System;
using System.Collections.Generic;
using System.IO;
using Core.Enums;
using WPFCommon.Helpers;

namespace Core.Constants
{
    public static class DirectoryConstants
    {
        /// <summary>
        /// Base dir to output various records
        /// </summary>
        public static string OutputDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ALC");

        public static string CsvOutputDir => Path.Combine(OutputDir, "CSV");

        /// <summary>
        /// Directory that stores general records
        /// </summary>
        public static string ProductionLineRecordDir => Path.Combine(OutputDir, "General");


        public static string FaiConfigDir2DLeft => Path.Combine(FaiConfigDir2D, "Cavity1");
        public static string FaiConfigDir2DRight => Path.Combine(FaiConfigDir2D, "Cavity2");

        public static string FaiConfigDir3DLeft => Path.Combine(FaiConfigDir3D, "Cavity1");
        public static string FaiConfigDir3DRight => Path.Combine(FaiConfigDir3D, "Cavity2");

        public static string FaiNamesDir => Path.Combine(FaiConfigDir, "FaiNames");
        public static string ErrorLogDir => Path.Combine(Directory.GetCurrentDirectory(), "Log");


        //TODO: remove the following dirs
        public static string ImageDirRight => Path.Combine(Directory.GetCurrentDirectory(), "Cavity2");
        public static string ImageDirLeft => Path.Combine(Directory.GetCurrentDirectory(), "Cavity1");


        public static string ConfigDir2D => Path.Combine(Directory.GetCurrentDirectory(), "2DConfigs");
        private static string ConfigDir2DMtm => Path.Combine(ConfigDir2D, "I40MTMConfig");
        private static string ConfigDir2DAlps => Path.Combine(ConfigDir2D, "I40ALPSConfig");


        public static Dictionary<ProductType, string> ConfigDirs2D { get; } = new Dictionary<ProductType, string>()
        {
            [ProductType.Mtm] = ConfigDir2DMtm,
            [ProductType.Alps] = ConfigDir2DAlps
        };

        private static string FaiConfigDir => Path.Combine(DirectoryHelper.ConfigDirectory, "Fai");
        private static string FaiConfigDir2D => Path.Combine(FaiConfigDir, "2D");

        /// <summary>
        /// Fai items config dir for Cavity1 and Cavity2
        /// Some tolerances may differ from type to type
        /// but not from cavity to cavity
        /// </summary>
        public static Dictionary<ProductType, string> FaiConfigDirs2D { get; } = new Dictionary<ProductType, string>()
        {
            [ProductType.Mtm] = Path.Combine(FaiConfigDir2D, ProductType.Mtm.ToString()),
            [ProductType.Alps] = Path.Combine(FaiConfigDir2D, ProductType.Alps.ToString())
        };


        private static string FaiConfigDir3D => Path.Combine(FaiConfigDir, "3D");
        private static string FaiConfigDir3DCavity1 => Path.Combine(FaiConfigDir3D, "Cavity1");
        private static string FaiConfigDir3DCavity2 => Path.Combine(FaiConfigDir3D, "Cavity2");

        public static Dictionary<ProductType, string> FaiConfigDirs3DCavity1 { get; } =
            new Dictionary<ProductType, string>()
            {
                [ProductType.Mtm] = Path.Combine(FaiConfigDir3DCavity1, ProductType.Mtm.ToString()),
                [ProductType.Alps] = Path.Combine(FaiConfigDir3DCavity1, ProductType.Alps.ToString())
            };

        public static Dictionary<ProductType, string> FaiConfigDirs3DCavity2 { get; } =
            new Dictionary<ProductType, string>()
            {
                [ProductType.Mtm] = Path.Combine(FaiConfigDir3DCavity2, ProductType.Mtm.ToString()),
                [ProductType.Alps] = Path.Combine(FaiConfigDir3DCavity2, ProductType.Alps.ToString())
            };
    }
}