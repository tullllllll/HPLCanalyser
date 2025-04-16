using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HPLC.Data;
using HPLC.ViewModels;
using HPLC.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HPLC;

public class App : Application
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
        using (var context = new HPLCDbContext())
        {
            context.Database.EnsureDeleted(); // Zorgt dat de db en tabellen bestaan
            context.Database.EnsureCreated(); // Zorgt dat de db en tabellen bestaan
        }
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