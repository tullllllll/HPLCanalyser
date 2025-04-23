using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using HPLC.ViewModels;

namespace HPLC.Views;

public partial class FileSelect : Window
{
    public FileSelect()
    {
        InitializeComponent();
        
        Dispatcher.UIThread.InvokeAsync(() =>
            this.FindControl<DataGrid>("FileSelectGrid").Columns[1].Sort());
        
        Dispatcher.UIThread.InvokeAsync(() =>
            this.FindControl<DataGrid>("FileSelectGrid").Columns[1].Sort());
    }

    public FileSelect(FileSelectViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}