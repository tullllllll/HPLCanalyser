using Avalonia.ReactiveUI;
using HLPC.ViewModels;

namespace HLPC.Views
{
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
