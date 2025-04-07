using Avalonia.Controls;
using HPLC.ViewModels;

namespace HPLC.Views;

public partial class GraphUserControl : UserControl
{
    public GraphUserControl(GraphUserControlViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}