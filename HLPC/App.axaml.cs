using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HLPC.Data;
using HLPC.Models;
using Microsoft.EntityFrameworkCore;
using HLPC.Views;

namespace HLPC;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                WindowState = WindowState.Maximized
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}