using System.Collections.Generic;
using Core.Enums;

namespace Core.Models
{
    public class LineChartData
    {
        public Dictionary<string, double> Data { get; set; }
        public LineChartUnitType UnitType { get; set; }
    }
}