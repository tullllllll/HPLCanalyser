using Avalonia.Controls;
using HLPC.ViewModels;

namespace HLPC.Views
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