using System.Collections.Generic;
using HalconDotNet;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Results
{
    public class GraphicPack3DViewModel : ViewModelBase
    {
        public List<HImage> Images { get; set; }

        public HObject Graphics { get; set; }

        public string ErrorMessage { get; set; }

        public Dictionary<string, double> FaiResults { get; set; }

        public bool ItemExists { get; set; }
    }
}