using System.Collections.ObjectModel;
using System.ComponentModel;
using HPLC.Models;
using HPLC.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HPLC.ViewModels;

public class GraphViewModel : INotifyPropertyChanged
{
    // Services
    private readonly DataSetService _dataSetService;
    
    // Variables
    public DataSet DataSet => _dataSetService.SelectedDataSet;
    public ObservableCollection<ObservablePoint> ObservablePoints { get; set; }
    public ObservableCollection<ISeries> SeriesCollection { get; set; }
    public Axis[] XAxes { get; set; } = {
        new Axis
        {
            Name = "Tijd in: ",
            TextSize = 14,
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
            MinLimit = 0,
            SeparatorsPaint = new SolidColorPaint
            {
                Color = SKColors.White
            }
        }
    };
    
    public GraphViewModel(DataSetService DataSetService)
    {
        _dataSetService = DataSetService;

        _dataSetService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(DataSetService.SelectedDataSet))
            {
                OnPropertyChanged(nameof(DataSet));
                UpdateChartData();
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
                Fill = null
            }
        };
        
        OnPropertyChanged(nameof(SeriesCollection));
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}