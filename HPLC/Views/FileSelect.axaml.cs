using System.Windows.Input;
using Avalonia.Controls;
using HPLC.ViewModels;

namespace HPLC.Views;

public partial class FileSelect : Window
{
    public FileSelect()
    {
        InitializeComponent();
    }

    public FileSelect(FileSelectViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}