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

    private DataSet _selectedReferenceDataSet;
    public DataSet SelectedReferenceDataSet
    {
        get => _selectedReferenceDataSet;
        set
        {
            if (_selectedReferenceDataSet != value)
            {
                _selectedReferenceDataSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedReferenceDataSet)));
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

        return dataPoints;
    }
    
    private List<DataPoint> ApplyBaselineCorrection(List<DataPoint> dataPoints)
    {
    ///Loop threw half a second worth of datapoint.values. take the average of points that are below zero
    return null;
    }
}