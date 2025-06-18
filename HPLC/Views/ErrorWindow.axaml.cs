using Avalonia.Controls;
using Avalonia.Interactivity;
using HPLC.ViewModels;

namespace HPLC.Views;

public partial class ErrorWindow : Window
{
    public ErrorWindow()
    {
        InitializeComponent();

        Opened += (_, __) => { OkButton.Focus(); };
    }

    public ErrorWindow(string message) : this()
    {
        DataContext = new ErrorViewModel(message);
    }

    private void Ok_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}