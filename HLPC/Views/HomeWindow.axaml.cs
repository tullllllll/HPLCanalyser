using System.Linq;
using Avalonia.Controls;
using HLPC.Data;
using HLPC.ViewModels;

namespace HLPC.Views;

public partial class HomeWindow : UserControl
{
    public HomeWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}