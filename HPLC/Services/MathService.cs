using System;
using System.Collections.Generic;
using System.Linq;
using HPLC.Models;

namespace HPLC.Services;

public class MathService
{

    ///bespreken met Christiaan
    /// <param name="windowSize">The size of the moving window used for averaging</param>
    public List<DataPoint> SmoothData(List<DataPoint> dataPoints, int windowSize = 5)
    {
        var smoothed = new List<DataPoint>();

        for (int i = 0; i < dataPoints.Count; i++)
        {
            int start = Math.Max(0, i - windowSize / 2);
            int end = Math.Min(dataPoints.Count - 1, i + windowSize / 2);

            double avgValue = dataPoints.Skip(start).Take(end - start + 1).Average(dp => dp.Value);
            
            smoothed.Add(new DataPoint
            {
                Time = dataPoints[i].Time,
                Value = avgValue
            });
        }
        return smoothed;
    }
    
    public List<Peak> DetectPeaks(List<DataPoint> dataPoints, double threshold, double minPeakWidth)
    {
        var peaks = new List<Peak>();

        bool inPeak = false;
        int peakStartIndex = -1;
        int peakMaxIndex = -1;
        double peakMaxValue = double.MinValue;
        double peakStartValue = 0; // To store the starting value of the peak


        for (int i = 0; i < dataPoints.Count; i++)
        {
            var dp = dataPoints[i];
            
            if (dp.Value <= threshold)
            {
                if (inPeak)
                {
                    peaks.Add(CreatePeak(dataPoints, peakStartIndex, i - 1, peakMaxIndex));
                    inPeak = false;
                }
                continue;
            }
            
            if (!inPeak)
            {
                inPeak = true;
                peakStartIndex = i;
                peakMaxIndex = i;
                peakMaxValue = dp.Value;
                peakStartValue = dp.Value;
                continue;
            }
            
            if (dp.Value > peakMaxValue)
            {
                peakMaxValue = dp.Value;
                peakMaxIndex = i;
            }

            if (peakStartValue * 1.45 > dp.Value)
            {
                peaks.Add(CreatePeak(dataPoints, peakStartIndex, i - 1, peakMaxIndex));
                inPeak = false;
            }
        }

        if (inPeak) peaks.Add(CreatePeak(dataPoints, peakStartIndex, dataPoints.Count - 1, peakMaxIndex));

        return peaks.Where(p => (p.EndTime - p.StartTime) >= minPeakWidth).ToList();
    }
    
    private Peak CreatePeak(List<DataPoint> dataPoints, int startIdx, int endIdx, int maxIdx)
    {
        var peakData = dataPoints.GetRange(startIdx, endIdx - startIdx + 1);
        double baseline = Math.Min(dataPoints[startIdx].Value, dataPoints[endIdx].Value);

        // Correctie voor baseline
        var correctedData = peakData.Select(dp => new DataPoint
        {
            Time = dp.Time,
            Value = Math.Max(0, dp.Value - baseline)
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
            PeakHeight = dataPoints[maxIdx].Value - baseline,
            Area = area,
            WidthAtHalfHeight = widthAtHalfHeight
        };
    }
    
     // --- Area onder piek met Trapeziumregel ---
    private double CalculateArea(List<DataPoint> dataPoints)
    {
        double area = 0;
        for (int i = 1; i < dataPoints.Count; i++)
        {
            var dp1 = dataPoints[i - 1];
            var dp2 = dataPoints[i];
            var time = (dp2.Time - dp1.Time);
            area += time * (dp1.Value + dp2.Value) / 2;
        }
        return area;
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