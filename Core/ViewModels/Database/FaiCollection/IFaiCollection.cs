﻿namespace Core.ViewModels.Database.FaiCollection
{
    public interface IFaiCollection
    {
         string InspectionTime { get; set; }

         int Cavity { get; set; }
         
         string Result { get; set; }
    }
}