﻿using System;

 namespace Core.ViewModels.Database.FaiCollection
{
    public interface IFaiCollection
    {
         DateTime InspectionTime { get; set; }

         int Cavity { get; set; }
         
         string Result { get; set; }
    }
}