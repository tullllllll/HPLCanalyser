using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using HPLC.Models;
using HPLC.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;

namespace HPLC.ViewModels;

public class GraphViewModel : INotifyPropertyChanged
{
    // Services
    private readonly DataSetService _dataSetService;
    private readonly MathService _mathService;
    
    // Variables
    public DataSet DataSet => _dataSetService.SelectedDataSet;
    public DataSet ReferenceDataSet => _dataSetService.SelectedReferenceDataSet;
    public ObservableCollection<ObservablePoint> ObservablePoints { get; set; }
    public ObservableCollection<ObservablePoint> ReferenceObservablePoints { get; set; }
    public ObservableCollection<ISeries> SeriesCollection { get; set; }
    public Axis[] XAxes { get; set; } = {
        new Axis
        {
            Name = "Tijd in: ",
            TextSize = 14,
            MinLimit = null,
            MaxLimit = null,
            SeparatorsPaint = new SolidColorPaint
            {
                Color = SKColors.White
            }
        }
    };
    public Axis[] YAxes { get; set; } = {
        new Axis
        {
            Name = "Variabele: ",
            MinLimit = null,
            MaxLimit = null,
            SeparatorsPaint = new SolidColorPaint
            {
                Color = SKColors.White
            },
            
            
        }
    };
    
    public GraphViewModel(DataSetService DataSetService, MathService MathService)
    {
        _dataSetService = DataSetService;
        _mathService = MathService;

        _dataSetService.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(DataSetService.SelectedDataSet):
                {
                    OnPropertyChanged(nameof(DataSet));
                    UpdateChartData();
                    break;
                }
                case nameof(DataSetService.SelectedReferenceDataSet):
                {
                    OnPropertyChanged(nameof(ReferenceDataSet));
                    UpdateReference();
                    break;
                }
            }
        };
        
        UpdateChartData();
    }

    private void UpdateChartData()
    {
        if (DataSet == null || DataSet.DataPoints == null) return;
        
        ObservablePoints = new ObservableCollection<ObservablePoint>();
        
        foreach (var dp in DataSet.DataPoints)
            ObservablePoints.Add(new ObservablePoint(dp.Time, dp.Value));
        
        SeriesCollection = new ObservableCollection<ISeries>
        {
            new LineSeries<ObservablePoint> (ObservablePoints)
            {
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null,
                Name = DataSet.Name
            },
        };
        
        OnPropertyChanged(nameof(SeriesCollection));
        DrawThemPeaks();
    }

    private void DrawThemPeaks()
    {
        var listoPeaks = _mathService.DetectPeaks(DataSet.DataPoints.ToList(),0,0);
    }

    public void UpdateReference()
    {
        if (SeriesCollection.Count > 1)
            SeriesCollection.RemoveAt(1);
        
        ReferenceObservablePoints = new ObservableCollection<ObservablePoint>();
        
        foreach (var dp in ReferenceDataSet.DataPoints)
            ReferenceObservablePoints.Add(new ObservablePoint(dp.Time, dp.Value));

        var newLine = new LineSeries<ObservablePoint>(ReferenceObservablePoints)
        {
            Fill = null,
            GeometryFill = null,
            GeometryStroke = null,
            Name = DataSet.Name
        };
        
        SeriesCollection.Add(newLine);
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}