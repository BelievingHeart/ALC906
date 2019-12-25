using System.Collections.Generic;

namespace Core.Constants
{
    public static class NameConstants
    {
        
        /// <summary>
        /// Names of all LJX-8000A controllers
        /// </summary>
        public static List<string> ControllerNames = new List<string>
        {
            "192.168.0.1@24691", "192.168.0.2@24691", "192.168.0.3@24691"
        };

        public static string TopCameraName = "Hikvision MV-CH250-20TM-F-NF (00D46076741)";

        public static readonly string SqlConnectionString =
            "Server=(LocalDb)\\Product;Database=I40;Trusted_Connection=True;";

        public static readonly string DateTimeFormat =
            "yy-MM-dd_HH-mm-ss_fff";


    }
}