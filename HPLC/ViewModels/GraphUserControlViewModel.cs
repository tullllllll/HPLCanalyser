using CommunityToolkit.Mvvm.Input;

namespace HPLC.ViewModels;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

public partial class GraphUserControlViewModel
{
    public ObservableCollection<ISeries> SeriesCollection { get; set; }
    public Axis[] Axes { get; set; }
    public ObservableCollection<ObservableValue> ObservableValues { get; set; }

    public GraphUserControlViewModel()
    {
        ObservableValues = new ObservableCollection<ObservableValue>();
        
        SeriesCollection = new ObservableCollection<ISeries>
        {
            new LineSeries<double> { Values = new double[] { 5, 2, 8, 3, 7, 1 } }
        };
    }
    
    public Axis[] XAxes { get; set; } =
    {
        new Axis
        {
            Name = "X-as Titel",
            LabelsRotation = 15,
            TextSize = 14,
            SeparatorsPaint = new SolidColorPaint
            {
                    Color = SKColors.White,
            },            
            NamePaint = new SolidColorPaint
            {
                Color = SKColors.Blue
            },
            LabelsPaint = new SolidColorPaint
            {
                Color = SKColors.Red
            }
        }
    };

    public Axis[] YAxes { get; set; } =
    {
        new Axis
        {
            Name = "Y-as Titel",
            MinLimit = 0,
            MaxLimit = 10,
            SeparatorsPaint = new SolidColorPaint
            {
                Color = SKColors.White,
            }, 
            NamePaint = new SolidColorPaint
            {
                Color = SKColors.Green
            },
            LabelsPaint = new SolidColorPaint
            {
                Color = SKColors.Purple
            }
        }
    };

    
    
    
    [RelayCommand]
    public void AddItem()
    {
        var newSeries = new LineSeries<double>
        {
            Values = new ObservableCollection<double> { 10, 16, 22 }, // Voeg hier je eigen waarden toe
            Fill = null
        };
        SeriesCollection.Add(newSeries);
    }




}