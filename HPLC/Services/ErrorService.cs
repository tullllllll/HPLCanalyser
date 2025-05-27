using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using HPLC.Views;

namespace HPLC.Services;

public static class ErrorService 
{
    public static async void CreateWindow(string message)
    {
        var window = new ErrorWindow(message);
        var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime!;
        await window.ShowDialog(lifetime.MainWindow);
    }
}