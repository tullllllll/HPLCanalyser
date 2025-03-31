using Avalonia.Controls;
using HPLC.ViewModels;

namespace HPLC.Views
{
    public partial class someGraph : UserControl
    {
        public someGraph()
        {
            InitializeComponent();
            DataContext = new GraphViewModel();
        }
    }
}