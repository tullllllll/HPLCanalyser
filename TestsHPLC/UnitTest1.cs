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
    public void CalculateWidthAtHalfHeight_ShouldReturnCorrectWidth()
    {
        // Arrange
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { Time = 0, Value = 0 },
            new DataPoint { Time = 1, Value = 2 },
            new DataPoint { Time = 2, Value = 4 },
            new DataPoint { Time = 3, Value = 2 },
            new DataPoint { Time = 4, Value = 0 }
        };
        var baseline = new Baseline(0, 0); // Assuming a flat baseline for simplicity

        // Act
        double result = _mathService.CalculateWidthAtHalfHeight(dataPoints, baseline);

        // Assert
        Assert.That(result, Is.EqualTo(2));        
    }
}