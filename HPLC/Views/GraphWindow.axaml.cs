using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using HPLC.ViewModels;
using LiveChartsCore.Kernel.Sketches;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using SkiaSharp;

namespace HPLC.Views;

public partial class GraphWindow : ReactiveUserControl<GraphViewModel>
{

    private readonly GraphViewModel _graphViewModel;
    
    public GraphWindow()
    {
        _graphViewModel = App.ServiceProvider.GetRequiredService<GraphViewModel>();
        
        InitializeComponent();
        
        this.WhenActivated((CompositeDisposable disposable) =>
        {
            _graphViewModel!.RequestChartExport.RegisterHandler(interaction =>
            {
                var chart = graphUserControl.ChartForSave;
                var window = this.GetVisualRoot() as Window;

                interaction.SetOutput((chart, window));
                return Task.CompletedTask;
            }).DisposeWith(disposable);
            _graphViewModel!.RequestPeakTableExport.RegisterHandler(interaction =>
            {
                var window = this.GetVisualRoot() as Window;

                interaction.SetOutput(("e",window));
                return Task.CompletedTask;
            }).DisposeWith(disposable);
        });
    }

    private void ColorView_OnColorChanged(object? sender, ColorChangedEventArgs e)
    {
        Debug.WriteLine(_graphViewModel!.Peaks);
        Debug.WriteLine(_graphViewModel!.SeriesCollection);
        if (sender is Control control && control.Tag is string Target)
        {
            var selectedColor = e.NewColor;
            var SkColor = new SKColor(selectedColor.R, selectedColor.G, selectedColor.B);
            _graphViewModel.UpdateLineColor(Target, SkColor);
        }
    }
    

}