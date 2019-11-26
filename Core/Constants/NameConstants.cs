using System.Collections.Generic;

namespace Core.Constants
{
    public static class NameConstants
    {
        public static List<string> FaiItemNames3D = new List<string>()
        {
            "16.1", "16.2",
            "18.E", "18.M",
            "20.1", "20.2", "20.3", "20.4",  
            "21", "22"
        };
        
        public static List<string> FaiItemNames2D = new List<string>()
        {
            "1-1A","1-1B","1-1C","1-2A","1-2B","1-2C",
            "2-1A","2-1B","2-1C","2-2A","2-2B","2-2C",
            "5-1A","5-2A",
            "6-1A","6-2A",
            "7-1A","7-1B","7-2A","7-2B",
            "8-1A","8-1B","8-2A","8-2B",
            "9-1","9-2"
        };

        /// <summary>
        /// Names of all LJX-8000A controllers
        /// </summary>
        public static List<string> ControllerNames = new List<string>()
        {
            "192.168.0.1@24691", "192.168.0.2@24691", "192.168.0.3@24691"
        };

        public static string TopCameraName = "Hikvision MV-CH250-20TM-F-NF (00D46076741)";
    }
}