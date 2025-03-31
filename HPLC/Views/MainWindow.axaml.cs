using Avalonia.Controls;
using HPLC.ViewModels;

namespace HPLC.Views
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
