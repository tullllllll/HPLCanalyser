using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using HPLC.Models;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using HPLC.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace HPLC.ViewModels
{
    public partial class GraphUserControlViewModel
    {
        public ObservableCollection<ISeries> SeriesCollection { get; set; }
        
        public bool IsSelectionModeActive { get; set; }
        
        // Properties for X and Y axes
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        // Constructor that initializes with DataSet
        public GraphUserControlViewModel(DataSet dataSet)
        {
            SeriesCollection = new ObservableCollection<ISeries>();

            // Extract data points for X and Y values
            var dataPoints = dataSet.DataPoints.ToList();
            var peaks = dataSet.Peaks.ToList();
            var minX = dataPoints.FirstOrDefault().Time;
            var maxX = dataPoints.LastOrDefault().Time;
            var minY = double.Min(dataPoints.Min(p => p.Value),0);
            var maxY = dataPoints.Max(p => p.Value);
            
            // Create the series
            var newSeries = new LineSeries<ObservablePoint>
            {
                Values = new ObservableCollection<ObservablePoint>(
                    dataPoints.Select(p => new ObservablePoint(p.Time, p.Value))
                ),
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0
            };
            var peakLine = new LineSeries<ObservablePoint>
            {
                Values = new ObservableCollection<ObservablePoint>(
                    dataPoints.Where(dp => dp.Time == peaks[0].StartTime || dp.Time == peaks[0].EndTime)
                        .Select(dp => new ObservablePoint(dp.Time, dp.Value))
                ),
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0
            };

            // Add the new series to the series collection
            SeriesCollection.Add(newSeries);
            SeriesCollection.Add(peakLine);
            Debug.WriteLine(MathService.GetPeakMaximum(peaks[0],dataSet));
            Debug.WriteLine(MathService.GetRetentionTime(peaks[0]));
            Debug.WriteLine(MathService.CalculatePeakArea(peaks[0],dataSet,true));
            // Dynamically set axis limits based on the data points
            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Time (Min)",
                    MinLimit = minX,
                    MaxLimit = maxX,
                    LabelsRotation = 15,
                    TextSize = 14,
                    SeparatorsPaint = new SolidColorPaint { Color = SKColors.White },
                    NamePaint = new SolidColorPaint { Color = SKColors.Blue },
                    LabelsPaint = new SolidColorPaint { Color = SKColors.Red }
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Value (mV)",
                    MinLimit = minY,
                    MaxLimit = maxY,
                    SeparatorsPaint = new SolidColorPaint { Color = SKColors.White },
                    NamePaint = new SolidColorPaint { Color = SKColors.Green },
                    LabelsPaint = new SolidColorPaint { Color = SKColors.Purple }
                }
            };
        }

        // RelayCommand to add a new item (series) to the chart

        [RelayCommand]
        public void ToggleSelectionMode()
        {
            IsSelectionModeActive = !IsSelectionModeActive;
            Debug.WriteLine($"Selection mode: {IsSelectionModeActive}");
        }

        public bool getSelectionMode()
        {
            return IsSelectionModeActive;
        }

        
    }
}
