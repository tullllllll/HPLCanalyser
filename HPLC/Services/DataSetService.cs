using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using HPLC.Models;
using Path = System.IO.Path;

namespace HPLC.Services;

public class DataSetService (SimpleKeyCRUDService<DataSet> dataSetService)
{
    public void ReadFile(string fileName, string fileContent)
    {
        string datapointString = fileContent.Substring(fileContent.ToLower().LastIndexOf("intensity", StringComparison.Ordinal)+9);
        
        var dataPoints = FormatFileContent(datapointString);
 
        dataSetService.Add(new DataSet()
        {
            Name = Path.GetFileNameWithoutExtension(fileName),
            Date_Added = DateTime.Now,
            DataPoints = dataPoints
        });
    }
    
    private List<DataPoint> FormatFileContent (string fileContent)
    {
        var dataPoints = new List<DataPoint>();
        var lines = fileContent.ReplaceLineEndings("\n").Split('\n');

        foreach (var line in lines)
        {
            var formattedLine = (Regex.Replace(line.Trim(), @"[\t; ]+", " ").Replace(",", ".")).Split(' ');

            if (formattedLine.Length == 2)
            {
                dataPoints.Add(new DataPoint()
                {
                    Time = double.Parse(formattedLine[0], CultureInfo.InvariantCulture),
                    Value = double.Parse(formattedLine[1], CultureInfo.InvariantCulture),
                });
            }
        }
        
        return dataPoints;
    }
}