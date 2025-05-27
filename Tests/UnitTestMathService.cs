using HPLC;
using HPLC.Models;
using HPLC.Services;

namespace TestsHPLC;

public class MathServiceTests
{
    private MathService _mathService;

    [SetUp]
    public void Setup()
    {
        _mathService = new MathService();
    }

    [Test]
    public void CalculateWidthAtHalfHeight_CompareBaselines()
    {
        // Arrange
        var dataPoints1 = new List<DataPoint>
        {
                new DataPoint { Time = 0, Value = 0 },
                new DataPoint { Time = 1, Value = 2-0.1 },
                new DataPoint { Time = 2, Value = 5-0.2 },
                new DataPoint { Time = 3, Value = 7-0.3 },
                new DataPoint { Time = 4, Value = 5-0.4 },
                new DataPoint { Time = 5, Value = 2-0.5 },
                new DataPoint { Time = 6, Value = 0-0.6 }
            };
        var baseline1 = new Baseline(-0.1, 0); // Negative Slope
        
        var dataPoints2 = new List<DataPoint>
        {
            new DataPoint { Time = 0, Value = 0 },
            new DataPoint { Time = 1, Value = 2+0.1},
            new DataPoint { Time = 2, Value = 5+0.2},
            new DataPoint { Time = 3, Value = 7+0.3},
            new DataPoint { Time = 4, Value = 5+0.4},
            new DataPoint { Time = 5, Value = 2+0.5},
            new DataPoint { Time = 6, Value = 0+0.6}
        };
        var baseline2 = new Baseline(0.1, 0); // Positive Slope

        // Act
        double result1 = _mathService.CalculateWidthAtHalfHeight(dataPoints1, baseline1);
        double result2 = _mathService.CalculateWidthAtHalfHeight(dataPoints2, baseline2);

        // Assert
        Assert.That(result1, Is.EqualTo(result2).Within(0.01));
    }

    [Test]
    public void Area_ZeroBaseline()
    {
        var dataPoints1 = new List<DataPoint>
        {
            new DataPoint { Time = 0, Value = 0 },
            new DataPoint { Time = 1, Value = 2 },
            new DataPoint { Time = 2, Value = 5 },
            new DataPoint { Time = 3, Value = 7 },
            new DataPoint { Time = 4, Value = 5 },
            new DataPoint { Time = 5, Value = 2 },
            new DataPoint { Time = 6, Value = 0 }
        };
        var baseline1 = new Baseline(0, 0); // Negative Slope
        double result = _mathService.CalculateArea(dataPoints1, baseline1);
        Assert.That(result, Is.EqualTo(21).Within(0.01));
    }
    [Test]
    public void Area_NegativeBaseline()
    {
        var dataPoints1 = new List<DataPoint>
        {
            new DataPoint { Time = 0, Value = 0 },
            new DataPoint { Time = 1, Value = 2-0.1 },
            new DataPoint { Time = 2, Value = 5-0.2 },
            new DataPoint { Time = 3, Value = 7-0.3 },
            new DataPoint { Time = 4, Value = 5-0.4 },
            new DataPoint { Time = 5, Value = 2-0.5 },
            new DataPoint { Time = 6, Value = 0-0.6 }
        };
        var baseline1 = new Baseline(-0.1, 0); // Negative Slope
        double result = _mathService.CalculateArea(dataPoints1, baseline1);
        Assert.That(result, Is.EqualTo(21).Within(0.01));
    }
    [Test]
    public void Area_PositiveBaseline()
    {
        var dataPoints1 = new List<DataPoint>
        {
            new DataPoint { Time = 0, Value = 0 },
            new DataPoint { Time = 1, Value = 2+0.1 },
            new DataPoint { Time = 2, Value = 5+0.2 },
            new DataPoint { Time = 3, Value = 7+0.3 },
            new DataPoint { Time = 4, Value = 5+0.4 },
            new DataPoint { Time = 5, Value = 2+0.5 },
            new DataPoint { Time = 6, Value = 0+0.6 }
        };
        var baseline1 = new Baseline(0.1, 0); // Negative Slope
        double result = _mathService.CalculateArea(dataPoints1, baseline1);
        Assert.That(result, Is.EqualTo(21).Within(0.01));
    }
}