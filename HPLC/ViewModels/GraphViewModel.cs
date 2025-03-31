using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace HPLC.ViewModels
{
    public class GraphViewModel
    {
        public ObservableCollection<ISeries> SeriesCollection { get; set; }

        public GraphViewModel()
        {
            SeriesCollection = new ObservableCollection<ISeries>
            {
                new LineSeries<double> { Values = new double[] { 3, 5, 7, 4, 2, 6 } },
                new LineSeries<double> { Values = new double[] { 5, 2, 8, 3, 7, 1 } }
            };
        }
    }
}