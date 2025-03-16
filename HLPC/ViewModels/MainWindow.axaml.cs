using Avalonia.Controls;

namespace HLPC;

public partial class MainWindow : Window
{
    public string Test { get; set; } = "test";
    public string Greeting => "Welcome to Avalonia! This is my added text.";


    public MainWindow()
    {
        InitializeComponent();
    }
}
