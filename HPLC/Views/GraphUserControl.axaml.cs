using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using HPLC.Models;
using HPLC.Services;
using HPLC.ViewModels;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;

namespace HPLC.Views;

public partial class GraphUserControl : UserControl
{
    public GraphUserControlViewModel ViewModel => (GraphUserControlViewModel)DataContext;
    public GraphUserControl(DataSet dataSet)
    {
        InitializeComponent();
        DataContext = new GraphUserControlViewModel(dataSet);
        Chart.DataPointerDown += OnDataPointerDown;
        _selectedPoints = new List<DataPoint>();
    }

    private List<DataPoint> _selectedPoints;

    private void OnDataPointerDown(IChartView chart, IEnumerable<ChartPoint> points)
    {
        if (!ViewModel.getSelectionMode()) return;
        var point = points.FirstOrDefault();
        if (point == null) return;

        var dataPoint = new DataPoint
        {
            Time = point.Coordinate.SecondaryValue, // X
            Value = point.Coordinate.PrimaryValue // Y
        };

        _selectedPoints.Add(dataPoint);

        Debug.WriteLine($"Selected point: Time = {dataPoint.Time}, Value = {dataPoint.Value}");

        if (_selectedPoints.Count == 2)
        {
            Debug.WriteLine("Two points selected:");
            Debug.WriteLine($"Point 1: Time = {_selectedPoints[0].Time}, Value = {_selectedPoints[0].Value}");
            Debug.WriteLine($"Point 2: Time = {_selectedPoints[1].Time}, Value = {_selectedPoints[1].Value}");
            _selectedPoints.Clear();
        }
    }
}