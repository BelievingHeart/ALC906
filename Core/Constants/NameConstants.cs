using System.Collections.Generic;

namespace Core.Constants
{
    public static class NameConstants
    {
        public static List<string> FaiItemNames = new List<string>()
        {
            "16.1", "16.2", 
            "17.1", "17.2", "17.3", "17.4",
            "18.E", "18.M",
            "19.1", "19.2", "19.3", "19.4", "19.5", "19.6", "19.7", "19.8", 
            "20.1", "20.2", "20.3", "20.4",  
            "21", "22"
        };

        /// <summary>
        /// Names of all LJX-8000A controllers
        /// </summary>
        public static List<string> ControllerNames = new List<string>()
        {
            "192.168.0.1@24691", "192.168.0.2@24691", "192.168.0.3@24691"
        };
    }
}