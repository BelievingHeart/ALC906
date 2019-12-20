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
        public static string OutputDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ALC"); }
        }

        public static string CsvOutputDir
        {
            get { return Path.Combine(OutputDir, "CSV"); }
        }

        /// <summary>
        /// Directory that stores general records
        /// </summary>
        public static string ProductionLineRecordDir
        {
            get { return Path.Combine(OutputDir, "General"); }
        }


        public static string ErrorLogDir
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "Log"); }
        }


        public static string ImageDir2D
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "2DImages"); }
        }

        public static string ImageDir3D
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "3DImages"); }
        }


        public static string ConfigDir2D
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "2DConfigs"); }
        }

        private static string ConfigDir2DMtm
        {
            get { return Path.Combine(ConfigDir2D, "I40MTMConfig"); }
        }

        private static string ConfigDir2DAlps
        {
            get { return Path.Combine(ConfigDir2D, "I40ALPSConfig"); }
        }


        public static Dictionary<ProductType, string> ConfigDirs2D { get; } = new Dictionary<ProductType, string>()
        {
            [ProductType.Mtm] = ConfigDir2DMtm,
            [ProductType.Alps] = ConfigDir2DAlps
        };

        private static string FaiConfigDir
        {
            get { return Path.Combine(DirectoryHelper.ConfigDirectory, "Fai"); }
        }


        private static string FaiConfigDir3D
        {
            get { return Path.Combine(FaiConfigDir, "3D"); }
        }

        private static string FaiConfigDir3DCavity1
        {
            get { return Path.Combine(FaiConfigDir3D, "Cavity1"); }
        }

        private static string FaiConfigDir3DCavity2
        {
            get { return Path.Combine(FaiConfigDir3D, "Cavity2"); }
        }

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