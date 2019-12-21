using System;

namespace Core.ViewModels.Database.FaiCollection
{
    public class FaiCollectionTest : IFaiCollection
    {
        public DateTime InspectionTime { get; set; }
        public int Cavity { get; set; }
        public string Result { get; set; }

        public string Test { get; set; }
    }
}