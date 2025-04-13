using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HPLC.Models;
using Path = System.IO.Path;

namespace HPLC.Services;

public class DataSetService (SimpleKeyCRUDService<DataSet> dataSetService)
{
    public event PropertyChangedEventHandler PropertyChanged;

    private DataSet _selectedDataSet;
    public DataSet SelectedDataSet
    {
        get => _selectedDataSet;
        set
        {
            if (_selectedDataSet != value)
            {
                _selectedDataSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDataSet)));
            }
        }
    }
    
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
        
        return ConvertToHalfSeconds(dataPoints);
    }
    
    private List<DataPoint> ConvertToHalfSeconds(List<DataPoint> dataPoints)
    {
        var groupedDataPoints = ApplyBaselineCorrection(dataPoints)
            .GroupBy(dp => Math.Floor(dp.Time / 0.5))
            .Select(group => new DataPoint
            {
                Time = group.Key * 0.5,
                Value = group.Average(dp => dp.Value)
            })
            .ToList();

        return groupedDataPoints;
    }
    
    private List<DataPoint> ApplyBaselineCorrection(List<DataPoint> dataPoints)
    {
        // Find the minimum value in the dataset
        var minValue = dataPoints.Min(dp => dp.Value);

        // If the minimum value is below 0, apply a correction
        if (minValue < 0)
        {
            var correctionFactor = Math.Abs(minValue);
            dataPoints = dataPoints.Select(dp => new DataPoint
            {
                Time = dp.Time,
                Value = dp.Value + correctionFactor // Shift values upwards
            }).ToList();
        }

        return dataPoints;
    }
}