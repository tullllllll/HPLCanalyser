using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HPLC.Models;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;

namespace HPLC.ViewModels;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia;

//TODO:
//Selector for reference data
//
public partial class GraphUserControlViewModel : ObservableObject
{
    public ObservableCollection<ISeries> SeriesCollection { get; set; }
    public Axis[] Axes { get; set; }
    public ObservableCollection<ObservablePoint> ObservableValues { get; set; }
    
    
    [ObservableProperty]
    private ObservableValue? _selectedValue;

    public GraphUserControlViewModel()
    {
        var dataPoints = DataPointGenerator();
        ObservableValues = new ObservableCollection<ObservablePoint>();

        foreach (var dataPoint in dataPoints)
        {   
            ObservableValues.Add(new ObservablePoint( dataPoint.Time,dataPoint.Value));
        }
        
        SeriesCollection = new ObservableCollection<ISeries>
        {
            new LineSeries<ObservablePoint> (ObservableValues)
            {
                Fill = null
            }
        };
    }
    
    public Axis[] XAxes { get; set; } =
    {
        new Axis
        {
            Name = "Tijd in: ",
            TextSize = 14,
            SeparatorsPaint = new SolidColorPaint
            {
                    Color = SKColors.White,
            }
        }
    };

    public Axis[] YAxes { get; set; } =
    {
        new Axis
        {
            Name = "Variabele",
            MinLimit = 0,
            SeparatorsPaint = new SolidColorPaint
            {
                Color = SKColors.White,
            }
        }
    };

    public void AddReference(List<Double> values)
    {
        var newSeries = new LineSeries<double>
        {
            Values = new ObservableCollection<double>(values),
            Fill = null,
            GeometryFill = null
        };
        SeriesCollection.Add(newSeries);
    }
    
    /// <summary>
    /// delete later, just for testing
    /// </summary>
    /// <returns></returns>
    public List<DataPoint> DataPointGenerator()
    {
        var random = new Random();
        var dataPoints = new List<DataPoint>();

        for (int i = 0; i < 10; i++)
        {
            dataPoints.Add(new DataPoint
            {
                ID = i,
                DataSetID = 1, // Assuming a default DataSetID
                Time = i,
                Value = random.Next(0,11),
                DataSet = new DataSet() // Assuming a default DataSet
            });
        }

        return dataPoints;
        
    }

}