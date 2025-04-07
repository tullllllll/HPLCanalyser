using Avalonia;
using Avalonia.Controls;
using HPLC.Models;
using HPLC.ViewModels;

namespace HPLC.Views;

public partial class GraphUserControl : UserControl
{
    public GraphUserControl(DataSet dataSet)
    {
        InitializeComponent();
        DataContext = new GraphUserControlViewModel(dataSet);
    }

    
}