using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HPLC.Data;
using HPLC.Models;
using Path = System.IO.Path;

namespace HPLC.Services;

public class DataSetService (SimpleKeyCRUDService<DataSet> dataSetService, HPLCDbContext context, NavigationService navigationService)
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
            DataPoints = dataPoints,
            Last_Used = DateTime.Now,
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

    public int GetLastInsertId()
    {
        return context.DataSet.OrderByDescending(e => e.ID)
            .Select(e => e.ID)
            .FirstOrDefault();
    }

    public void DeleteDataSet(int datasetId)
    {
        dataSetService.Delete(datasetId);
    }
    
    public void SetActiveDataSet(int dataSetId)
    {
        SelectedDataSet = dataSetService.GetWithChildren(dataSetId);
        SelectedDataSet.Last_Used = DateTime.Now;
        context.SaveChanges();
        navigationService.Navigate("Graph");
    }

    public void SetReferenceDataSet(int dataSetId)
    {
        SelectedReferenceDataSet = dataSetService.GetWithChildren(dataSetId);
        SelectedReferenceDataSet.Last_Used = DateTime.Now;
        context.SaveChanges();
    }
}