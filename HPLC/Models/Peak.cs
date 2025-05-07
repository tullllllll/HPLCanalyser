namespace HPLC.Models;

public class Peak
{
    public double StartTime { get; set; }
    public double PeakTime { get; set; }
    public double EndTime { get; set; }
    public double PeakHeight { get; set; }
    public double Area { get; set; }
    public double WidthAtHalfHeight { get; set; }
    public string Name { get; set; }
}