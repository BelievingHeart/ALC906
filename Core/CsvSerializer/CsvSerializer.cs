using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.CsvSerializer
{
    public class CsvSerializer
    {
        public List<string> Header { get; set; }
        public string OutputDir { get; set; }

        public CsvSerializer(string outputDir)
        {
            OutputDir = outputDir;
            _csvPath = UpdateSerializePath();
        }

        public void Serialize(IEnumerable<ICsvColumnElement> items, string imageName, bool shouldSerialize = true, IEnumerable<string> filters = null)
        {
            if(!shouldSerialize) return;


            var csvColumnElements =
                filters == null ? items.ToList() : items.Where(ele => filters.Any(ele.CsvName.Contains));
            
            if (Header == null) InitHeader(csvColumnElements.Select(item => item.CsvName));

            var line = csvColumnElements.Select(item => item.Value.ToString("f4")).ToList();
            line.Insert(0, imageName);
            var csvLine = string.Join(",", line);

            var fileExists = File.Exists(_csvPath);
            Directory.CreateDirectory(OutputDir);
            var lineToWrite = fileExists ? csvLine : HeaderLine + Environment.NewLine + csvLine;
            using (var fs = new StreamWriter(_csvPath, fileExists))
            {
                fs.WriteLine(lineToWrite);
            }
        }

        private void InitHeader(IEnumerable<string> names)
        {
            Header = new List<string>(){"Time"};
            foreach (var name in names)
            {
                Header.Add(name);
            }
        }

        public string HeaderLine
        {
            get { return string.Join(",", Header); }
        }

        private string _csvPath;

        private string UpdateSerializePath()
        {
            return Path.Combine(OutputDir, DateTime.Now.ToString("MMdd-HHmmss-ffff")) + ".csv";
        }


        /// <summary>
        /// Calculate the min, max average values of csv columns
        /// </summary>
        public void SummariseCsv()
        {
            if (!File.Exists(_csvPath)) return;
            
            var lines = File.ReadAllLines(_csvPath);
            var summaries = GetCsvSummary(lines);
            
            var maxs = string.Join(",",summaries.Item1);
            var mins = string.Join(",",summaries.Item2);
            var averages = string.Join(",",summaries.Item3);
            var maxDiffs = string.Join(",",summaries.Item4);
            var maxIndecies = string.Join(",",summaries.Item5);
            var minIndecies = string.Join(",",summaries.Item6);
            var ngIndicators = string.Join(",", summaries.Item7);
            // Reserve space for the time column
            maxs = "max," + maxs;
            mins = "min," + mins;
            averages = "average," + averages;
            maxDiffs = "max diff," + maxDiffs;
            maxIndecies = "max index," + maxIndecies;
            minIndecies = "min index," + minIndecies;
            ngIndicators = "Result:," + ngIndicators;
            
            
            
            using (var fs = new StreamWriter(_csvPath, append:true))
            {
                fs.WriteLine("");
                fs.WriteLine(maxs);
                fs.WriteLine(mins);
                fs.WriteLine(averages);
                fs.WriteLine(maxDiffs);
                fs.WriteLine(maxIndecies);
                fs.WriteLine(minIndecies);
                if(RepeatibilityMode) fs.WriteLine(ngIndicators);
            }
            
            

            _csvPath = UpdateSerializePath();
        }


        public bool RepeatibilityMode { get; set; } = true;
        
        /// <summary>
        /// Summary an in-memory representation of csv file
        /// </summary>
        /// <param name="strs">Lines of comma-separated strings</param>
        /// <returns>Max, min, average and max-diff</returns>
    static Tuple<List<double>, List<double>, List<double>, List<double>, List<int>, List<int>, List<string>> GetCsvSummary(IEnumerable<string> strs)  
    {
        // Skip the header line
        strs = strs.Skip(1);
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/how-to-compute-column-values-in-a-csv-text-file-linq
        IEnumerable<IEnumerable<double>> multiColQuery =  
            from line in strs  
            let elements = line.Split(',')  
            // Skip the first column
            let scores = elements.Skip(1)  
            select (from str in scores  
                    select Convert.ToDouble(str));  
  
        // Execute the query and cache the results to improve  
        // performance.   
        // ToArray could be used instead of ToList.  
        var results = multiColQuery.ToList();  
  
        // Find out how many columns you have in results.  
        int columnCount = results[0].Count();

        var maxs = new List<double>();
        var mins = new List<double>();
        var averages = new List<double>();
        var maxDiffs = new List<double>();
        var maxIndex = new List<int>();
        var minIndex = new List<int>();
        var NgInicators = new List<string>();
  
        // Perform aggregate calculations Average, Max, and  
        // Min on each column.              
        // Perform one iteration of the loop for each column   
        // of scores.  
        // You can use a for loop instead of a foreach loop   
        // because you already executed the multiColQuery   
        // query by calling ToList.  
        for (int column = 0; column < columnCount; column++)  
        {  
            var columnValues = from row in results  
                           select row.ElementAt(column);  
            double average = columnValues.Average();  
            double max = columnValues.Max();  
            double min = columnValues.Min();
            double maxDiff = max - min;

            maxs.Add(max);
            mins.Add(min);
            averages.Add(average);
            maxDiffs.Add(maxDiff);
            maxIndex.Add(IndexVeryClose(columnValues, max));
            minIndex.Add(IndexVeryClose(columnValues, min));
            var NgIndicator = maxDiff > NgThreshold ? "NG" : "";
            NgInicators.Add(NgIndicator);
        }  
        return new Tuple<List<double>, List<double>, List<double>, List<double>, List<int>, List<int>, List<string>>(maxs, mins, averages, maxDiffs, maxIndex, minIndex, NgInicators);
    }

        public static double NgThreshold { get; set; } = 0.01;


        private static double _epslon = 0.00001;
        private static bool DoubleVeryClose(double actual, double expected)
        {
            return Math.Abs(actual - expected) < _epslon;
        }

        /// <summary>
        /// Return the index of the element that is very close to the target value
        /// </summary>
        /// <param name="values"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static int IndexVeryClose(IEnumerable<double> values, double target)
        {
            var list = values.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (DoubleVeryClose(list[i], target)) return i;
            }

            return -1;
        }
    }
}