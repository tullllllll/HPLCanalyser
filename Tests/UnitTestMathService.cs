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
            new() { Time = 0, Value = 0 },
            new() { Time = 1, Value = 2 - 0.1 },
            new() { Time = 2, Value = 5 - 0.2 },
            new() { Time = 3, Value = 7 - 0.3 },
            new() { Time = 4, Value = 5 - 0.4 },
            new() { Time = 5, Value = 2 - 0.5 },
            new() { Time = 6, Value = 0 - 0.6 }
        };
        var baseline1 = new Baseline(-0.1, 0); // Negative Slope

        var dataPoints2 = new List<DataPoint>
        {
            new() { Time = 0, Value = 0 },
            new() { Time = 1, Value = 2 + 0.1 },
            new() { Time = 2, Value = 5 + 0.2 },
            new() { Time = 3, Value = 7 + 0.3 },
            new() { Time = 4, Value = 5 + 0.4 },
            new() { Time = 5, Value = 2 + 0.5 },
            new() { Time = 6, Value = 0 + 0.6 }
        };
        var baseline2 = new Baseline(0.1, 0); // Positive Slope

        // Act
        var result1 = _mathService.CalculateWidthAtHalfHeight(dataPoints1, baseline1);
        var result2 = _mathService.CalculateWidthAtHalfHeight(dataPoints2, baseline2);

        // Assert
        Assert.That(result1, Is.EqualTo(result2).Within(0.01));
    }

    [Test]
    public void Area_ZeroBaseline()
    {
        var dataPoints1 = new List<DataPoint>
        {
            new() { Time = 0, Value = 0 },
            new() { Time = 1, Value = 2 },
            new() { Time = 2, Value = 5 },
            new() { Time = 3, Value = 7 },
            new() { Time = 4, Value = 5 },
            new() { Time = 5, Value = 2 },
            new() { Time = 6, Value = 0 }
        };
        var baseline1 = new Baseline(0, 0); // Negative Slope
        var result = _mathService.CalculateArea(dataPoints1, baseline1);
        Assert.That(result, Is.EqualTo(21).Within(0.01));
    }

    [Test]
    public void Area_NegativeBaseline()
    {
        var dataPoints1 = new List<DataPoint>
        {
            new() { Time = 0, Value = 0 },
            new() { Time = 1, Value = 2 - 0.1 },
            new() { Time = 2, Value = 5 - 0.2 },
            new() { Time = 3, Value = 7 - 0.3 },
            new() { Time = 4, Value = 5 - 0.4 },
            new() { Time = 5, Value = 2 - 0.5 },
            new() { Time = 6, Value = 0 - 0.6 }
        };
        var baseline1 = new Baseline(-0.1, 0); // Negative Slope
        var result = _mathService.CalculateArea(dataPoints1, baseline1);
        Assert.That(result, Is.EqualTo(21).Within(0.01));
    }

    [Test]
    public void Area_PositiveBaseline()
    {
        var dataPoints1 = new List<DataPoint>
        {
            new() { Time = 0, Value = 0 },
            new() { Time = 1, Value = 2 + 0.1 },
            new() { Time = 2, Value = 5 + 0.2 },
            new() { Time = 3, Value = 7 + 0.3 },
            new() { Time = 4, Value = 5 + 0.4 },
            new() { Time = 5, Value = 2 + 0.5 },
            new() { Time = 6, Value = 0 + 0.6 }
        };
        var baseline1 = new Baseline(0.1, 0); // Negative Slope
        var result = _mathService.CalculateArea(dataPoints1, baseline1);
        Assert.That(result, Is.EqualTo(21).Within(0.01));
    }
}