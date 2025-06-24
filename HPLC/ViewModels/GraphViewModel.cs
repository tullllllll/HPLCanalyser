using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using DynamicData;
using HPLC.Models;
using HPLC.Services;
using HPLC.Views;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
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
    
    private List<(ChartPoint,ObservablePoint)> _selectedPoints = new List<(ChartPoint,ObservablePoint)>();

    private bool _IsSelectionModeActive;
    public bool IsSelectionModeActive
    {
        get => _IsSelectionModeActive;
        set
        {
            if (_IsSelectionModeActive != value)
            {
                _IsSelectionModeActive = value;
                OnPropertyChanged(nameof(IsSelectionModeActive));

                if (!_IsSelectionModeActive) // Selection mode turned off
                {
                    _selectedPoints = new List<(ChartPoint,ObservablePoint)>();
                    Debug.WriteLine(SeriesCollection.ToString());

                    var series = SeriesCollection.FirstOrDefault(x => x.Name == "Point 1");
                    if (series != null)
                    {
                        SeriesCollection.Remove(series);
                    }
                }
            }
        }
    }

    private ZoomAndPanMode _panningMode = ZoomAndPanMode.Both;
    public ZoomAndPanMode PanningMode
    {
        get => _panningMode;
        set
        {
            if (_panningMode != value)
            {
                _panningMode = value;
                OnPropertyChanged(nameof(PanningMode)); // or RaisePropertyChanged if you're using MVVM frameworks
            }
        }
    }

    private double _threshold = 60; 
    public double Threshold
    {
        get => _threshold;
        set
        {
            if (_threshold != value)
            {
                _threshold = value;
                OnPropertyChanged(nameof(Threshold));
                DrawThemPeaks(_threshold, MinPeakWidth);
            }

        }
    }

    private double _minPeakWidth { get; set; } = 0.1;

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

    private double _startpointBaseline = 0;
    public double StartpointBaseline
    {
        get => _startpointBaseline;
        set
        {
            if (_startpointBaseline != value)
            {
                _startpointBaseline = value;
                OnPropertyChanged(nameof(StartpointBaseline));
                UpdateBaseline();
            }
        }
    }
    
    private double _endpointBaseline = 1.5;
    public double EndpointBaseline
    {
        get => _endpointBaseline;
        set
        {
            if (_endpointBaseline != value)
            {
                _endpointBaseline = value;
                OnPropertyChanged(nameof(EndpointBaseline));
                UpdateBaseline();
            }
        }
    }
    
    public double PointsForBaseline
    {
        get => EndpointBaseline - StartpointBaseline;
        set
        {
            return;
        }
    }

    private Baseline _baseline { get; set; }

    public Axis[] XAxes { get; set; } =
    {
        new Axis
        {
            Name = "Time (min): ",
            TextSize = 14,
            MinLimit = null,
            MaxLimit = null,
            ShowSeparatorLines = false
        }
    };

    public Axis[] YAxes { get; set; } =
    {
        new Axis
        {
            Name = "Intensity (µV):",
            TextSize = 14,
            MinLimit = null,
            MaxLimit = null,
            ShowSeparatorLines = false,
            TicksPaint = new SolidColorPaint { Color = SKColors.Black }
        }
    };

    // Command
    public ICommand DeletePeakCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveImageCommand { get; }
    public ReactiveCommand<Unit, Unit> SavePeakTableCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleBaselineCommand { get; }
    public ReactiveCommand<IEnumerable<ChartPoint>, Unit> DataPointerDownCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleSelectionModeCommand { get; }
    
    // Interaction
    public Interaction<Unit, (CartesianChart chart, Window parent)> RequestChartExport { get; } = new();
    public Interaction<Unit, (string a, Window parent)> RequestPeakTableExport { get; } = new();

    public GraphViewModel(DataSetService dataSetService, MathService mathService)
    {
        _dataSetService = dataSetService;
        _mathService = mathService;

        _dataSetService.PropertyChanged += HandlePropertyChanged;
        DeletePeakCommand = ReactiveCommand.Create<Peak>(DeletePeak);
        SaveImageCommand = ReactiveCommand.CreateFromTask(SaveImage);
        SavePeakTableCommand = ReactiveCommand.CreateFromTask(SavePeakTable);
        ToggleBaselineCommand = ReactiveCommand.Create(ShowBaseline);
        DataPointerDownCommand = ReactiveCommand.Create<IEnumerable<ChartPoint>>(GetPointerPoints);
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

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = "output",
            DefaultExtension = "jpg",
            Title = "Save Image",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new FilePickerFileType("JPEG Image (*.jpg)")
                {
                    Patterns = new[] { "*.jpg" }
                },
                new FilePickerFileType("JPEG Image (*.jpeg)")
                {
                    Patterns = new[] { "*.jpeg" }
                },
                new FilePickerFileType("PNG Image (*.png)")
                {
                    Patterns = new[] { "*.png" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*.*" }
                }
            }
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

    private async Task SavePeakTabletWithDialogAsync(ObservableCollection<Peak> peaks, Window window)
    {
        var storage = window.StorageProvider;

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = DataSet.Name + " Peak Table",
            DefaultExtension = "csv",
            Title = "Save Peak Data to CSV",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new FilePickerFileType("CSV Files")
                {
                    Patterns = new[] { "*.csv" }
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*.*" }
                }
            }
        });

        if (file is not null)
        {
            var csvLines = new List<string>
            {
                "Name; Start Time (min); Peak Time (min); End Time (min); Total Time (min); Height (mV); Area (mV.min); Width ½ Height (min)"
            };

            foreach (var peak in peaks)
            {
                string line = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}; {1:F3}; {2:F3}; {3:F3}; {4:F3}; {5:F2}; {6:F1}; {7:F6}",
                    peak.Name,
                    peak.StartTime,
                    peak.PeakTime,
                    peak.EndTime,
                    peak.Time,
                    peak.PeakHeight,
                    peak.Area,
                    peak.WidthAtHalfHeight
                );
                csvLines.Add(line);
            }

            await using var stream = File.OpenWrite(file.Path.LocalPath);
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            foreach (var line in csvLines)
            {
                await writer.WriteLineAsync(line);
            }
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
            .Cast<object>()
            .FirstOrDefault(series =>
                (series is LineSeries<ObservablePoint> obs && obs.Tag?.ToString() == peak.Tag) ||
                (series is LineSeries<ObservablePoint, DiamondGeometry> other && other.Tag?.ToString() == peak.Tag));

        if (lineToRemove != null)
        {
            if (lineToRemove is LineSeries<ObservablePoint> lineSeries1)
                SeriesCollection.Remove(lineSeries1);

            if (lineToRemove is LineSeries<ObservablePoint, DiamondGeometry> lineSeries2)
                SeriesCollection.Remove(lineSeries2);
        }

        OnPropertyChanged(nameof(Peaks));
        OnPropertyChanged(nameof(SeriesCollection));
    }

    private async Task SaveImage()
    {
        var (chart, window) = await RequestChartExport.Handle(Unit.Default).FirstAsync();

        if (chart != null && window != null)
        {
            await SaveChartWithDialogAsync(chart, window);
        }
    }

    private async Task SavePeakTable()
    {
        var (e,window) = await RequestPeakTableExport.Handle(Unit.Default).FirstAsync();
        var peaks = Peaks;

        if (peaks != null && window != null)
        {
            await SavePeakTabletWithDialogAsync(peaks, window);
        }
    }

    public void ShowBaseline()
    {
        var dataPoints = DataSet.DataPoints.ToList();
        double dTime = dataPoints[1].Time - dataPoints[0].Time;
        double lastTime = dataPoints[^1].Time;

        if (BaseLineIsVisible)
        {
            RemoveBaseline();
        }

        var baseline = new LineSeries<ObservablePoint>
        {
            Values = new ObservableCollection<ObservablePoint>
            {
                new ObservablePoint(0, _baseline.GetBaseline(0, dTime)),
                new ObservablePoint(lastTime, _baseline.GetBaseline(lastTime, dTime))
            },
            Stroke = new SolidColorPaint(SKColors.Gray, 2),
            Fill = null,
            GeometryFill = null,
            GeometryStroke = null,
            ZIndex = 1,
            LineSmoothness = 0,
            Name = "BaseLine",
            Tag = "Baseline"
        };

        SeriesCollection.Add(baseline);
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
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3 },
            },
        };
        
        XAxes.First().MinLimit = 0;
        XAxes.First().MaxLimit = null;
        YAxes.First().MinLimit = null;
        YAxes.First().MaxLimit = null;

        var dataPoints = DataSet.DataPoints.ToList();
        double dTime = dataPoints[1].Time - dataPoints[0].Time;
        _baseline = Baseline.CalculateBaseline(dataPoints, dTime, PointsForBaseline,(int)Math.Floor(StartpointBaseline/dTime));
        
        OnPropertyChanged(nameof(SeriesCollection));
        DrawThemPeaks(Threshold, MinPeakWidth);
        if (BaseLineIsVisible)
            ShowBaseline();
    }

    private void DrawThemPeaks(double treshhold, double minPeakWidth, bool detectPeaks = true)
    {
        if (DataSet == null || DataSet.DataPoints == null) return;
        var detectedPeaks = detectPeaks? _mathService.DetectPeaks(DataSet.DataPoints.ToList(), treshhold, minPeakWidth, _baseline) : Peaks.ToList();

        var linesToRemove = SeriesCollection
            .Where(line => line.Tag?.ToString() != "Main" && line.Tag?.ToString() != "Reference" && line.Tag?.ToString() != "Baseline")
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
                        .Where(dp => _mathService.AboutEqual(dp.Time,peak.StartTime,0.001) || _mathService.AboutEqual(dp.Time,peak.EndTime,0.001))
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

        if (existingReference != null) SeriesCollection.Remove(existingReference);
        
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
            ZIndex = 3,
            GeometryFill = null,
            GeometryStroke = null,
            Name = ReferenceDataSet.Name,
            Tag = "Reference",
            Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 3 }
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
            if ((string)obsLine.Tag != "Reference" && (string)obsLine.Tag != "Main")
                obsLine.GeometryStroke = c;
        }
        else if (line is LineSeries<ObservablePoint, DiamondGeometry> otherLine)
        {
            otherLine.Stroke = c;
            if ((string)otherLine.Tag != "Reference" && (string)otherLine.Tag != "Main")
                otherLine.GeometryStroke = c;
        }
    }

    private bool _BaselineIsVisible = false;
    public bool BaseLineIsVisible
    {
        get => _BaselineIsVisible;
        set
        {
            _BaselineIsVisible = value;
            OnPropertyChanged(nameof(BaseLineIsVisible));
            if (value)
                ShowBaseline();
            else
                RemoveBaseline();
        }
    }

    public void UpdateBaseline()
    {
        var dataPoints = DataSet.DataPoints.ToList();
        double dTime = dataPoints[1].Time - dataPoints[0].Time;
        _baseline = Baseline.CalculateBaseline(dataPoints, dTime, PointsForBaseline,(int)Math.Floor(StartpointBaseline/dTime));
        
        DrawThemPeaks(Threshold, MinPeakWidth);
        
        if (BaseLineIsVisible)
            ShowBaseline();
    }
    
    private void RemoveBaseline()
    {
        var existingBaseline = SeriesCollection.FirstOrDefault(s => (string)s.Tag == "Baseline");

        if (existingBaseline != null) SeriesCollection.Remove(existingBaseline);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void GetPointerPoints(IEnumerable<ChartPoint> Points)
    {
        
        if (!IsSelectionModeActive)
        {
            PanningMode = ZoomAndPanMode.Both;
            return;
        }
        
        var minLimit = XAxes.First().MinLimit;
        if (minLimit != null) XAxes.First().MinLimit = Math.Max((double)minLimit, 0);
        PanningMode = ZoomAndPanMode.ZoomX;
        
        var firstPoint = Points.FirstOrDefault();
        if (firstPoint == null) return;
        if (firstPoint.Context.Series.Name != DataSet.Name) return;
        
        var x = firstPoint.Coordinate.SecondaryValue;
        var y = firstPoint.Coordinate.PrimaryValue;
        
        var selectedPoint = new ObservablePoint(x, y);
        
        // Add point if not already selected (optional: avoid duplicates)
        if (_selectedPoints.Count < 2)
        {
            _selectedPoints.Add((firstPoint,selectedPoint));
            SeriesCollection.Add(new LineSeries<ObservablePoint>
            {
                Values = [new ObservablePoint(x, y)],
                Name = "Point " + _selectedPoints.Count
            });
        }

        // Once two points are selected, call your peak method
        if (_selectedPoints.Count == 2)
        {
            if (_selectedPoints[0].Item1.Index > _selectedPoints[1].Item1.Index) _selectedPoints.Reverse(0,2);
            Peak newPeak = _mathService.CreatePeak(DataSet.DataPoints.ToList(), _baseline, _selectedPoints[0].Item1.Index, _selectedPoints[1].Item1.Index);
            newPeak.Color = Colors.Coral;
            newPeak.Tag = Peaks.Count.ToString();
            Peaks.Add(newPeak);
            DrawThemPeaks(_threshold, _minPeakWidth,false);
            _selectedPoints.Clear();
        }
    }
    
}