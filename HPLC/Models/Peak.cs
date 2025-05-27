using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using ReactiveUI;

namespace HPLC.Models;

public class Peak : ReactiveObject
{
    public double StartTime { get; set; }
    public double PeakTime { get; set; }
    public double EndTime { get; set; }
    public double PeakHeight { get; set; }
    public double Time
    {
        get => (EndTime - StartTime);
    }
    public double Area { get; set; }
    [NotMapped]
    public string Tag { get; set; }
    public double WidthAtHalfHeight { get; set; }
    private string _name;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
}