using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HPLC.Models;

namespace HPLC.Services;

public class MathService
{
    public List<Peak> DetectPeaks(List<DataPoint> dataPoints, double threshold, double minPeakWidth)
    {
        var peaks = new List<Peak>();

        bool inPeak = false;
        int peakStartIndex = -1;
        int peakMaxIndex = -1;
        double peakMaxValue = double.MinValue;
        (double a, double b) = CalculateBaseline(dataPoints);
        double Baseline(double time) => a * time * 2 * 60 + b;

        for (int i = 0; i < dataPoints.Count; i++)
        {
            var dp = dataPoints[i];
            var baseline = Baseline(dp.Time);

            if (!inPeak)
            {
                // Start peak if current value significantly above baseline
                if (dp.Value > baseline + threshold)
                {
                    inPeak = true;
                    peakStartIndex = i;
                    peakMaxIndex = i;
                    peakMaxValue = dp.Value;
                }
            }
            else
            {
                if (dp.Value > peakMaxValue)
                {
                    peakMaxValue = dp.Value;
                    peakMaxIndex = i;
                }

                // End peak as soon as value drops near baseline
                if (dp.Value <= baseline + threshold)
                {
                    peaks.Add(CreatePeak(dataPoints, peakStartIndex, i, peakMaxIndex));
                    inPeak = false;
                }
            }
        }

        // Close last peak if still open
        if (inPeak)
        {
            peaks.Add(CreatePeak(dataPoints, peakStartIndex, dataPoints.Count - 1, peakMaxIndex));
        }

        return peaks.Where(p => (p.EndTime - p.StartTime) >= minPeakWidth).ToList();
        
    }
    
    private Peak CreatePeak(List<DataPoint> dataPoints, int startIdx, int endIdx, int maxIdx)
    {
        var peakData = dataPoints.GetRange(startIdx, endIdx - startIdx + 1);
        // Correctie voor baseline
        var correctedData = peakData.Select(dp => new DataPoint
        {
            Time = dp.Time,
            Value = dp.Value
        }).ToList();

        // Area onder de piek berekenen
        double area = CalculateArea(correctedData);

        // Breedte bij halve hoogte berekenen
        double widthAtHalfHeight = CalculateWidthAtHalfHeight(correctedData);

        return new Peak
        {
            StartTime = dataPoints[startIdx].Time,
            PeakTime = dataPoints[maxIdx].Time,
            EndTime = dataPoints[endIdx].Time,
            PeakHeight = dataPoints[maxIdx].Value,
            Area = area,
            WidthAtHalfHeight = widthAtHalfHeight,
            Name = "Peak at " + dataPoints[maxIdx].Time.ToString("0.00")
        };
    }
    
     // --- Area onder piek ---
    private double CalculateArea(List<DataPoint> dataPoints)
    {
        //double baseline = CalculateBaseline(dataPoints);
        double area = 0;
        for (int i = 1; i < dataPoints.Count; i++)
        {
            var dp1 = dataPoints[i - 1];
            var dp2 = dataPoints[i];
            var time = (dp2.Time - dp1.Time);
            area += ((dp1.Value + dp2.Value) / 2) * time;
        }
        return area;
    }

    private (double,double) CalculateBaseline(List<DataPoint> dataPoints)
    {
        if (dataPoints.Count < 60)
        {
            ErrorService.CreateWindow("womp womp");
        }

        // Helper to get average point from a slice
        (double avgTime, double avgValue) Avg(int start)
        {
            var slice = dataPoints.Skip(start).Take(20).ToList();
            double avgTime = slice.Average(dp => dp.Time*60*2);
            double avgValue = slice.Average(dp => dp.Value);
            return (avgTime, avgValue);
        }

        // Get the three average points
        var (x1, y1) = Avg(0);    // First 20
        var (x2, y2) = Avg(30);   // Second 20
        var (x3, y3) = Avg(60);   // Third 20

        // Fit a line using linear regression on the three points
        double[] xs = { x1, x2, x3 };
        double[] ys = { y1, y2, y3 };

        double sumX = xs.Sum();
        double sumY = ys.Sum();
        double sumXY = xs.Zip(ys, (x, y) => x * y).Sum();
        double sumX2 = xs.Sum(x => x * x);
        int n = 3;

        double denominator = n * sumX2 - sumX * sumX;
        if (Math.Abs(denominator) < 1e-8)
            return (0, ys.Average()); // fallback: flat line

        double a = (n * sumXY - sumX * sumY) / denominator;
        double b = (sumY - a * sumX) / n;

        return (a, b);
    }
    
    // --- Breedte bij halve hoogte ---
    private double CalculateWidthAtHalfHeight(List<DataPoint> dataPoints)
    {
        if (dataPoints == null || dataPoints.Count < 2) return 0;

        var maxPoint = dataPoints.OrderByDescending(dp => dp.Value).First();
        int maxIndex = GetMaxPointIndex(dataPoints, maxPoint);
        double halfHeight = maxPoint.Value / 2;

        // Zoek naar links (vanaf de piek naar 0)
        DataPoint left = null;
        for (int i = maxIndex; i > 0; i--)
        {
            if (dataPoints[i].Value >= halfHeight && dataPoints[i - 1].Value <= halfHeight)
            {
                left = Interpolate(dataPoints[i], dataPoints[i - 1], halfHeight);
                break;
            }
        }

        // Zoek naar rechts (vanaf de piek naar einde)
        DataPoint right = null;
        for (int i = maxIndex; i < dataPoints.Count - 1; i++)
        {
            if (dataPoints[i].Value >= halfHeight && dataPoints[i + 1].Value <= halfHeight)
            {
                right = Interpolate(dataPoints[i], dataPoints[i + 1], halfHeight);
                break;
            }
        }

        if (left == null || right == null) return 0;

        return right.Time - left.Time;
    }

    private DataPoint Interpolate(DataPoint p1, DataPoint p2, double targetValue)
    {
        if (Math.Abs(p2.Time - p1.Time) < double.Epsilon)
            return new DataPoint { Time = p1.Time, Value = targetValue };

        double slope = (p2.Value - p1.Value) / (p2.Time - p1.Time);
        double timeAtTarget = p1.Time + (targetValue - p1.Value) / slope;
        return new DataPoint { Time = timeAtTarget, Value = targetValue };
    }

    private int GetMaxPointIndex(List<DataPoint> dataPoints, DataPoint maxPoint)
    {
        return dataPoints.FindIndex(dp => dp.Time == maxPoint.Time && dp.Value == maxPoint.Value);
    }

}