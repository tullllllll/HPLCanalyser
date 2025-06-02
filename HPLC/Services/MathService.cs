using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        double dTime = dataPoints[1].Time - dataPoints[0].Time;
        Baseline baseline = Baseline.CalculateBaseline(dataPoints,dTime);

        for (int i = 0; i < dataPoints.Count; i++)
        {
            var dp = dataPoints[i];
            var baselineAtPoint = baseline.GetBaseline(dp.Time,dTime);

            if (!inPeak)
            {
                // Start peak if current value significantly above baseline
                if (dp.Value > baselineAtPoint + threshold)
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
                if (dp.Value <= baselineAtPoint + threshold)
                {
                    peaks.Add(CreatePeak(dataPoints, baseline, peakStartIndex, i, peakMaxIndex));
                    inPeak = false;
                }
            }
        }

        // Close last peak if still open
        if (inPeak)
        {
            peaks.Add(CreatePeak(dataPoints, baseline, peakStartIndex, dataPoints.Count - 1, peakMaxIndex));
        }

        return peaks.Where(p => (p.EndTime - p.StartTime) >= minPeakWidth).ToList();
        
    }
    
    public Peak CreatePeak(List<DataPoint> dataPoints, Baseline baseline, int startIdx, int endIdx, int maxIdx)
    {
        var peakData = dataPoints.GetRange(startIdx, endIdx - startIdx + 1);
        
        // Area onder de piek berekenen
        double area = CalculateArea(peakData,baseline);

        // Breedte bij halve hoogte berekenen
        double widthAtHalfHeight = CalculateWidthAtHalfHeight(peakData, baseline);

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
    public double CalculateArea(List<DataPoint> dataPoints, Baseline baseline)
    {
        double area = 0;
        for (int i = 1; i < dataPoints.Count; i++)
        {
            var dp1 = dataPoints[i - 1];
            var dp2 = dataPoints[i];
            var time = (dp2.Time - dp1.Time);
            
            var baselineDp1 = baseline.GetBaseline(dp1.Time,time);
            var baselineDp2 = baseline.GetBaseline(dp2.Time,time);
            var heightDp1 = dp1.Value - baselineDp1;
            var heightDp2 = dp2.Value - baselineDp2;
            
            area += ((heightDp1 + heightDp2) / 2) * time;
        }
       
        return area;
    }
    
    // --- Breedte bij halve hoogte ---
    public double CalculateWidthAtHalfHeight(List<DataPoint> dataPoints, Baseline baseline)
    {
        if (dataPoints == null || dataPoints.Count < 2) return 0;

        var maxPoint = dataPoints.OrderByDescending(dp => dp.Value).First();
        int maxIndex = GetMaxPointIndex(dataPoints, maxPoint);
        double dTime = dataPoints[1].Time - dataPoints[0].Time;
        double baselineValueMaxPoint = baseline.GetBaseline(maxPoint.Time, dTime);
        double halfHeight = (maxPoint.Value-baselineValueMaxPoint) / 2;
        
        // Zoek naar links (vanaf de piek naar 0)
        DataPoint left = null;
        for (int i = maxIndex; i > 0; i--)
        {
            double baselineValuePoint = baseline.GetBaseline(dataPoints[i].Time, dTime);
            double adjustedHalfHeight = baselineValuePoint + halfHeight;
            if (dataPoints[i].Value >= adjustedHalfHeight && dataPoints[i - 1].Value <= adjustedHalfHeight)
            {
                left = Interpolate(dataPoints[i], dataPoints[i - 1], adjustedHalfHeight);
                break;
            }
        }

        // Zoek naar rechts (vanaf de piek naar einde)
        DataPoint right = null;
        for (int i = maxIndex; i < dataPoints.Count - 1; i++)
        {
            double baselineValuePoint = baseline.GetBaseline(dataPoints[i].Time, dTime);
            double adjustedHalfHeight = baselineValuePoint + halfHeight;
            if (dataPoints[i].Value >= adjustedHalfHeight && dataPoints[i + 1].Value <= adjustedHalfHeight)
            {
                right = Interpolate(dataPoints[i], dataPoints[i + 1], adjustedHalfHeight);
                break;
            }
        }
        
        if (left == null || right == null) return 0;
        return right.Time - left.Time;
    }

    public DataPoint Interpolate(DataPoint p1, DataPoint p2, double targetValue)
    {
        if (Math.Abs(p2.Time - p1.Time) < double.Epsilon)
            return new DataPoint { Time = p1.Time, Value = targetValue };

        double slope = (p2.Value - p1.Value) / (p2.Time - p1.Time);
        double timeAtTarget = p1.Time + (targetValue - p1.Value) / slope;
        return new DataPoint { Time = timeAtTarget, Value = targetValue };
    }

    public int GetMaxPointIndex(List<DataPoint> dataPoints, DataPoint maxPoint)
    {
        return dataPoints.FindIndex(dp => dp.Time == maxPoint.Time && dp.Value == maxPoint.Value);
    }

}