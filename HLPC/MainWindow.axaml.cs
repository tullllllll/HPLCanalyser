using Avalonia.Controls;

namespace HLPC;

public partial class MainWindow : Window
{
    public string Test { get; set; } = "test";

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }
}
