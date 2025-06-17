using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HPLC.ViewModels;

namespace HPLC.Views;

public partial class ErrorWindow : Window
{
    public ErrorWindow()
    {
        InitializeComponent();

        this.Opened += (_, __) =>
        {
            OkButton.Focus();
        };
    }

    public ErrorWindow(string message) : this()
    {
        DataContext = new ErrorViewModel(message);
    }

    private void Ok_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}