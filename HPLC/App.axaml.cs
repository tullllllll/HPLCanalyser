using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HPLC.Data;
using HPLC.Models;
using HPLC.ViewModels;
using Microsoft.EntityFrameworkCore;
using HPLC.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HPLC;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Register all the services needed for the application to run
        var collection = new ServiceCollection();
        collection.AddCommonServices();
        
        // Creates a ServiceProvider containing services from the provided IServiceCollection
        var services = collection.BuildServiceProvider();
        
        var viewModel = services.GetRequiredService<MainViewModel>();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                WindowState = WindowState.Maximized,
                DataContext = viewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}