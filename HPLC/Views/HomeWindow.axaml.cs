using System.Linq;
using Avalonia.Controls;
using HPLC.Data;
using HPLC.ViewModels;

namespace HPLC.Views;

public partial class HomeWindow : UserControl
{
    public HomeWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}