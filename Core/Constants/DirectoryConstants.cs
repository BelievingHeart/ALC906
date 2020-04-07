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
        


        public static string ErrorLogDir = "D:\\ALC-Logs";

        public static readonly string ImageBaseDir = "D:\\ALC-Images";

        public static string ImageDir2D
        {
            get { return Path.Combine(ImageBaseDir, "2D"); }
        }

        public static string ImageDir3D
        {
            get { return Path.Combine(ImageBaseDir, "3D"); }
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

        public static string ConfigDirectory = "D:\\ALC-Configs";

        private static string FaiConfigDir
        {
            get { return Path.Combine(ConfigDirectory, "Fai"); }
        }
        
        private static string DatabaseLimitsDir
        {
            get { return Path.Combine(ConfigDirectory, "DatabaseLimits"); }
        }
        
        public static string ShapeModelPath2D
        {
            get { return Path.Combine(ConfigDirectory, "ShapeModel/2D"); }
        }

        public static string ShapeModelPath3D
        {
            get { return Path.Combine(ConfigDirectory, "ShapeModel/3D"); }
        }
        
        
        public static Dictionary<ProductType, string> DatabaseLimitsDirs { get; } = new Dictionary<ProductType, string>()
        {
            [ProductType.Mtm] = Path.Combine(DatabaseLimitsDir, ProductType.Mtm.ToString()),
            [ProductType.Alps] = Path.Combine(DatabaseLimitsDir, ProductType.Alps.ToString())
        };
        
        private static string BackupDir => "D:\\ALC-Backups";
        public static string BackupDir3DConfigs => Path.Combine(BackupDir, "3D");

        public static string FaiConfigDir3D
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

        public static string TimeLineSerializationDir => System.IO.Path.Combine(ErrorLogDir, "Timelines");
    }
}