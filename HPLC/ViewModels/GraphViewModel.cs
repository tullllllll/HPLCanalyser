using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using HPLC.Models;
using HPLC.Services;
using HPLC.Views;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using Size = Avalonia.Size;

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
    public ObservableCollection<Peak> Peaks { get; set; } = new ObservableCollection<Peak>();
    public ObservableCollection<Peak> ReferencePeaks { get; set; } = new ObservableCollection<Peak>();

    private double _threshold = 60; // Default value
    public double Threshold
    {
        get => _threshold;
        set
        {
            if (_threshold != value)
            {
                _threshold = value;
                OnPropertyChanged(nameof(Threshold));
                DrawThemPeaks(_threshold, MinPeakWidth); // Update peaks when threshold changes
            }

        }
    }    
    public double _minPeakWidth { get; set; } = 0.1; // Standaardwaarde
    public double MinPeakWidth
    {
        get => _minPeakWidth;
        set
        {
            if (_minPeakWidth != value)
            {
                _minPeakWidth = value;
                OnPropertyChanged(nameof(MinPeakWidth));
                DrawThemPeaks(_threshold, _minPeakWidth); 
            }

        }
    }  
    
    // X and Y axis
    public Axis[] XAxes { get; set; } = {
        new Axis
        {
            Name = "Time (min): ",
            TextSize = 14,
            MinLimit = 0,
            MaxLimit = null,
            ShowSeparatorLines = false
        }
    };
    public Axis[] YAxes { get; set; } = {
        new Axis
        {
            Name = "Variable (mV):",
            TextSize = 14,
            MinLimit = null,
            MaxLimit = null,
            ShowSeparatorLines = false,
            TicksPaint = new SolidColorPaint{Color = SKColors.Black}
        }
    };
    
    // Command
    public ICommand DeletePeakCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveImageCommand { get; }
    
    // Interaction
    public Interaction<Unit, (CartesianChart chart, Window parent)> RequestChartExport { get; } = new();

    public GraphViewModel(DataSetService dataSetService, MathService mathService)
    {
        _dataSetService = dataSetService;
        _mathService = mathService;
        
        _dataSetService.PropertyChanged += HandlePropertyChanged;
        DeletePeakCommand = ReactiveCommand.Create<Peak>(DeletePeak);
        SaveImageCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var (chart, window) = await RequestChartExport.Handle(Unit.Default).FirstAsync();

            if (chart != null && window != null)
            {
                await SaveChartWithDialogAsync(chart, window);
            }
        });

        UpdateChartData();
    }
    
    private async Task SaveChartWithDialogAsync(CartesianChart chart, Window window)
    {
        var storage = window.StorageProvider;
        
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            SuggestedFileName = "output",
            DefaultExtension = "jpg",
            Title = "Save Image",
        });
        
        if (file is not null)
        {
            int width = (int)chart.Bounds.Width;
            int height = (int)chart.Bounds.Height;
            var bitmap = new RenderTargetBitmap(new PixelSize(width, height));
            
            chart.Background = new SolidColorBrush(Colors.White);
            bitmap.Render(chart);
            
            using var fileStream = File.OpenWrite(file.Path.LocalPath);
            bitmap.Save(fileStream);
        }
    }
    
    private void UnsubscribeFromPeak(Peak peak)
    {
        peak.PropertyChanged -= Peak_PropertyChanged;
    }
    
    private void DeletePeak(Peak peak)
    {
        if (peak == null) return;

        UnsubscribeFromPeak(peak);
        Peaks.Remove(peak);

        
        var lineToRemove = SeriesCollection
            .OfType<LineSeries<ObservablePoint>>()
            .FirstOrDefault(series => series.Tag?.ToString() == peak.Tag);

        if (lineToRemove != null)
        {
            SeriesCollection.Remove(lineToRemove);
        }

        OnPropertyChanged(nameof(Peaks));
        OnPropertyChanged(nameof(SeriesCollection));
    }

    private void Peak_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is Peak peak)
        {
            SeriesCollection.FirstOrDefault(el => el.Tag?.ToString() == peak.Tag).Name = peak.Name;
        }
    }
    
    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DataSetService.SelectedDataSet):
                OnPropertyChanged(nameof(DataSet));
                UpdateChartData();
                break;

            case nameof(DataSetService.SelectedReferenceDataSet):
                OnPropertyChanged(nameof(ReferenceDataSet));
                UpdateReference();
                break;
        }
    }

    private void UpdateChartData()
    {
        ObservablePoints = new ObservableCollection<ObservablePoint>();
        Peaks = new ObservableCollection<Peak>();
        OnPropertyChanged(nameof(Peaks));
        
        if (DataSet == null || DataSet.DataPoints == null)
        {
            SeriesCollection = new ObservableCollection<ISeries>();
            OnPropertyChanged(nameof(SeriesCollection));
            return; 
        }
        
        foreach (var dp in DataSet.DataPoints)
            ObservablePoints.Add(new ObservablePoint(dp.Time, dp.Value));
        
        SeriesCollection = new ObservableCollection<ISeries>
        {
            new LineSeries<ObservablePoint>(ObservablePoints)
            {
                Fill = null,
                ZIndex = 2,
                GeometryFill = null,
                GeometryStroke = null,
                Name = DataSet.Name,
                Tag = "Main",
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3},
            },
        };
        
        XAxes.First().MinLimit = null;
        XAxes.First().MaxLimit = null;
        YAxes.First().MinLimit = null;
        YAxes.First().MaxLimit = null;
        
        OnPropertyChanged(nameof(SeriesCollection));
        DrawThemPeaks(Threshold, MinPeakWidth);
    }

    private void DrawThemPeaks(double treshhold, double minPeakWidth)
    {
        if (DataSet == null || DataSet.DataPoints == null) return;
        
        var detectedPeaks = _mathService.DetectPeaks(DataSet.DataPoints.ToList(), treshhold, minPeakWidth);

        var linesToRemove = SeriesCollection
            .Where(line => line.Tag?.ToString() != "Main" && line.Tag?.ToString() != "Reference")
            .ToList();

        if (Peaks.Count > 0)
        {
            Peaks.Clear();
        }
        foreach (var line in linesToRemove)
        {
            SeriesCollection.Remove(line);
        }
        
        for (int i = 0; i < detectedPeaks.Count; i++)
        {
            var peak = detectedPeaks[i];
            peak.Tag = i.ToString();
            peak.Color = Colors.Coral;
            SKColor skColor = new SKColor(peak.Color.R, peak.Color.G, peak.Color.B, peak.Color.A);
            var peakLine = new LineSeries<ObservablePoint, DiamondGeometry>
            {
                Values = new ObservableCollection<ObservablePoint>(
                    DataSet.DataPoints
                        .Where(dp => dp.Time == peak.StartTime || dp.Time == peak.EndTime)
                        .Select(dp => new ObservablePoint(dp.Time, dp.Value))
                ),
                Stroke = new SolidColorPaint(skColor, 3),
                GeometryStroke = new SolidColorPaint(skColor, 3),
                Fill = null,
                GeometryFill = null,
                LineSmoothness = 0,
                Name = peak.Name,
                Tag = i.ToString(),
            };
            
            SeriesCollection.Add(peakLine);
            Peaks.Add(peak);
            peak.PropertyChanged += Peak_PropertyChanged;
        }
        OnPropertyChanged(nameof(SeriesCollection));
    }

    private void UpdateReference()
    {
        var existingReference = SeriesCollection
            .OfType<LineSeries<ObservablePoint>>()
            .FirstOrDefault(series => series.Tag?.ToString() == "Reference");

        if (existingReference != null)
        {
            SeriesCollection.Remove(existingReference);
        }
        
        ReferenceObservablePoints = new ObservableCollection<ObservablePoint>();
        if (ReferenceDataSet == null) return;
        
        foreach (var dp in ReferenceDataSet.DataPoints)
            ReferenceObservablePoints.Add(new ObservablePoint(dp.Time, dp.Value));

        var newLine = new LineSeries<ObservablePoint>(ReferenceObservablePoints)
        {
            Fill = null,
            ZIndex = 1,
            GeometryFill = null,
            GeometryStroke = null,
            Name = ReferenceDataSet.Name,
            Tag = "Reference",
            Stroke = new SolidColorPaint(SKColors.Red) {StrokeThickness = 3}
        };
        
        SeriesCollection.Add(newLine);
    }
    
    public void UpdateLineColor(string line_name, SKColor color)
    {
        if (SeriesCollection == null || SeriesCollection.Count == 0) return;
        
        var line = SeriesCollection
            .Cast<object>()
            .FirstOrDefault(series =>
                (series is LineSeries<ObservablePoint> obs && obs.Tag?.ToString() == line_name) ||
                (series is LineSeries<ObservablePoint, DiamondGeometry> other && other.Tag?.ToString() == line_name));

  
        var c = new SolidColorPaint(color) { StrokeThickness = 3 };
        if (line is LineSeries<ObservablePoint> obsLine)
        {
            obsLine.Stroke = c;
            obsLine.GeometryStroke = c;
        }
        else if (line is LineSeries<ObservablePoint, DiamondGeometry> otherLine)
        {
            otherLine.Stroke = c;
            otherLine.GeometryStroke = c;
        }

    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}