using Avalonia.Controls;
using HPLC.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace HPLC.Views;

public partial class GraphWindow : UserControl
{
    private readonly GraphViewModel _graphViewModel;
    
    public GraphWindow()
    {
        _graphViewModel = App.ServiceProvider.GetRequiredService<GraphViewModel>();
        
        InitializeComponent();
    }

    private void ColorView_OnColorChanged(object? sender, ColorChangedEventArgs e)
    {
        if (sender is Control control && control.Tag is string Target)
        {
            var selectedColor = e.NewColor;
            var SkColor = new SKColor(selectedColor.R, selectedColor.G, selectedColor.B);
            _graphViewModel.UpdateLineColor(Target, SkColor);
        }
    }
}