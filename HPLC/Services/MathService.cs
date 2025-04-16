using System;
using System.Collections.Generic;
using System.Linq;
using HPLC.Models;

namespace HPLC.Services;

public static class MathService
{
    public static double GetRetentionTime(DataPoint startPoint, DataPoint endPoint)
    {
        return endPoint.Time - startPoint.Time;
    }

    public static double GetRetentionTime(Peak peak)
    {
        return peak.EndTime - peak.StartTime;
    }

    public static double CalculatePeakArea(Peak peak, DataSet dataSet,Boolean baselineCorrected = false)
    {
        var dataPoints = dataSet.DataPoints.ToList();

        var startIndex = dataPoints.FindIndex(p => p.Time == peak.StartTime);
        var endIndex = dataPoints.FindIndex(p => p.Time == peak.EndTime);

        if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            return 0;

        double baseline = baselineCorrected? GetPeakMinimum(peak, dataSet) : 0;
        double area = 0;

        for (int i = startIndex; i < endIndex; i++)
        {
            var p1 = dataPoints[i];
            var p2 = dataPoints[i + 1];

            double adjustedMin = Math.Min(p1.Value, p2.Value) - baseline;
            double adjustedMax = Math.Max(p1.Value, p2.Value) - baseline;
            double timeDelta = GetRetentionTime(p1, p2);
            
            area += timeDelta * adjustedMin + timeDelta * (adjustedMax - adjustedMin) / 2;
        }

        return area;
    }

    public static double GetPeakMaximum(Peak peak, DataSet dataSet)
    {
        var dataPoints = GetPointsInRange(peak, dataSet);
        return dataPoints.Max(p => p.Value);
    }

    public static double GetPeakMinimum(Peak peak, DataSet dataSet)
    {
        var dataPoints = GetPointsInRange(peak, dataSet);
        return dataPoints.Min(p => p.Value);
    }

    private static List<DataPoint> GetPointsInRange(Peak peak, DataSet dataSet)
    {
        var dataPoints = dataSet.DataPoints.ToList();

        var startIndex = dataPoints.FindIndex(p => p.Time == peak.StartTime);
        var endIndex = dataPoints.FindIndex(p => p.Time == peak.EndTime);

        if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            return new List<DataPoint>();

        return dataPoints.GetRange(startIndex, endIndex - startIndex + 1);
    }
}
