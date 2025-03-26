using Avalonia.Controls;
using HLPC.ViewModels;

namespace HLPC.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
