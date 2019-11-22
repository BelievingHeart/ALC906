﻿namespace Core.CsvSerializer
{
    public interface ICsvColumnElement
    {
         string CsvName { get; }
         
         double Value { get; set; }
    }
}