using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HPLC.ViewModels;
using HPLC.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HPLC;

public class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Register all the services needed for the application to run
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCommonServices();

        // Creates a ServiceProvider containing services from the provided IServiceCollection
        ServiceProvider = serviceCollection.BuildServiceProvider();

        var viewModel = ServiceProvider.GetRequiredService<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                WindowState = WindowState.Maximized,
                DataContext = viewModel
            };

            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        base.OnFrameworkInitializationCompleted();
    }
}