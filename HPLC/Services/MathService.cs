using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using HPLC.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HPLC.Services;

public class MathService
{
    public List<Peak> DetectPeaks(List<DataPoint> dataPoints, double threshold, double minPeakWidth, Baseline baseline)
    {
        var peaks = new List<Peak>();

        var inPeak = false;
        var peakStartIndex = -1;
        var peakMaxIndex = -1;
        var peakMaxValue = double.MinValue;
        var dTime = dataPoints[1].Time - dataPoints[0].Time;

        for (var i = 0; i < dataPoints.Count; i++)
        {
            var dp = dataPoints[i];
            var baselineAtPoint = baseline.GetBaseline(dp.Time, dTime);

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
        if (inPeak) peaks.Add(CreatePeak(dataPoints, baseline, peakStartIndex, dataPoints.Count - 1, peakMaxIndex));

        return peaks.Where(p => p.EndTime - p.StartTime >= minPeakWidth).ToList();
    }

    public Peak CreatePeak(List<DataPoint> dataPoints, Baseline baseline, int startIdx, int endIdx, int? maxIdx = null)
    {
        var peakData = dataPoints.GetRange(startIdx, endIdx - startIdx + 1);
        if (maxIdx == null)
        {
            var maxValue = peakData.Max(point => point.Value);
            var localMaxIdx = peakData.FindIndex(point => point.Value == maxValue);
            maxIdx = startIdx + localMaxIdx;
        }

        // Area onder de piek berekenen
        var area = CalculateArea(peakData, baseline);

        // Breedte bij halve hoogte berekenen
        var widthAtHalfHeight = CalculateWidthAtHalfHeight(peakData, baseline);

        return new Peak
        {
            StartTime = dataPoints[startIdx].Time,
            PeakTime = dataPoints[maxIdx.Value].Time,
            EndTime = dataPoints[endIdx].Time,
            PeakHeight = dataPoints[maxIdx.Value].Value,
            Area = area,
            WidthAtHalfHeight = widthAtHalfHeight,
            Name = "Peak at " + dataPoints[maxIdx.Value].Time.ToString("0.00")
        };
    }

    // --- Area onder piek ---
    public double CalculateArea(List<DataPoint> dataPoints, Baseline baseline)
    {
        double area = 0;
        for (var i = 1; i < dataPoints.Count; i++)
        {
            var dp1 = dataPoints[i - 1];
            var dp2 = dataPoints[i];
            var time = dp2.Time - dp1.Time;

            var baselineDp1 = baseline.GetBaseline(dp1.Time, time);
            var baselineDp2 = baseline.GetBaseline(dp2.Time, time);
            var heightDp1 = dp1.Value - baselineDp1;
            var heightDp2 = dp2.Value - baselineDp2;

            area += (heightDp1 + heightDp2) / 2 * time;
        }

        return area;
    }

    // --- Breedte bij halve hoogte ---
    public double CalculateWidthAtHalfHeight(List<DataPoint> dataPoints, Baseline baseline)
    {
        if (dataPoints == null || dataPoints.Count < 2) return 0;

        var maxPoint = dataPoints.OrderByDescending(dp => dp.Value).First();
        var maxIndex = GetMaxPointIndex(dataPoints, maxPoint);
        var dTime = dataPoints[1].Time - dataPoints[0].Time;
        var baselineValueMaxPoint = baseline.GetBaseline(maxPoint.Time, dTime);
        var halfHeight = (maxPoint.Value - baselineValueMaxPoint) / 2;

        // Zoek naar links (vanaf de piek naar 0)
        DataPoint left = null;
        for (var i = maxIndex; i > 0; i--)
        {
            var baselineValuePoint = baseline.GetBaseline(dataPoints[i].Time, dTime);
            var adjustedHalfHeight = baselineValuePoint + halfHeight;
            if (dataPoints[i].Value >= adjustedHalfHeight && dataPoints[i - 1].Value <= adjustedHalfHeight)
            {
                left = Interpolate(dataPoints[i], dataPoints[i - 1], adjustedHalfHeight);
                break;
            }
        }

        // Zoek naar rechts (vanaf de piek naar einde)
        DataPoint right = null;
        for (var i = maxIndex; i < dataPoints.Count - 1; i++)
        {
            var baselineValuePoint = baseline.GetBaseline(dataPoints[i].Time, dTime);
            var adjustedHalfHeight = baselineValuePoint + halfHeight;
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

        var slope = (p2.Value - p1.Value) / (p2.Time - p1.Time);
        var timeAtTarget = p1.Time + (targetValue - p1.Value) / slope;
        return new DataPoint { Time = timeAtTarget, Value = targetValue };
    }

    public int GetMaxPointIndex(List<DataPoint> dataPoints, DataPoint maxPoint)
    {
        return dataPoints.FindIndex(dp => dp.Time == maxPoint.Time && dp.Value == maxPoint.Value);
    }

    public bool AboutEqual(double time1, double time2, double epsilon)
    {
        return Math.Abs(time1 - time2) < epsilon;
    }

    public void DrawPeaks(
        ObservableCollection<ISeries> seriesCollection,
        ObservableCollection<Peak> peaks,
        DataSet dataSet,
        double threshold,
        double minPeakWidth,
        Baseline baseline,
        bool detectPeaks,
        Func<double, double, double, bool> aboutEqual)
    {
        if (dataSet == null || dataSet.DataPoints == null) return;

        var detectedPeaks = detectPeaks
            ? DetectPeaks(dataSet.DataPoints.ToList(), threshold, minPeakWidth, baseline)
            : peaks.ToList();

        var linesToRemove = seriesCollection
            .Where(line => line.Tag?.ToString() != "Main" && line.Tag?.ToString() != "Reference" &&
                           line.Tag?.ToString() != "Baseline")
            .ToList();

        peaks.Clear();
        foreach (var line in linesToRemove) seriesCollection.Remove(line);

        for (var i = 0; i < detectedPeaks.Count; i++)
        {
            var peak = detectedPeaks[i];
            peak.Tag = i.ToString();
            peak.Color = Colors.Coral;
            var skColor = new SKColor(peak.Color.R, peak.Color.G, peak.Color.B, peak.Color.A);

            var peakLine = new LineSeries<ObservablePoint, DiamondGeometry>
            {
                Values = new ObservableCollection<ObservablePoint>(
                    dataSet.DataPoints
                        .Where(dp =>
                            aboutEqual(dp.Time, peak.StartTime, 0.001) || aboutEqual(dp.Time, peak.EndTime, 0.001))
                        .Select(dp => new ObservablePoint(dp.Time, dp.Value))
                ),
                Stroke = new SolidColorPaint(skColor, 3),
                GeometryStroke = new SolidColorPaint(skColor, 3),
                Fill = null,
                GeometryFill = null,
                LineSmoothness = 0,
                Name = peak.Name,
                Tag = i.ToString()
            };

            seriesCollection.Add(peakLine);
            peaks.Add(peak);
        }
    }

    public Baseline UpdateBaseline(DataSet dataSet, int pointsForBaseline)
    {
        var dataPoints = dataSet.DataPoints.ToList();
        var dTime = dataPoints[1].Time - dataPoints[0].Time;
        return Baseline.CalculateBaseline(dataPoints, dTime, pointsForBaseline);
    }

    public void ShowBaseline(
        ObservableCollection<ISeries> seriesCollection,
        Baseline baseline,
        DataSet dataSet,
        bool isVisible)
    {
        if (!isVisible) return;

        var dataPoints = dataSet.DataPoints.ToList();
        var dTime = dataPoints[1].Time - dataPoints[0].Time;
        var lastTime = dataPoints[^1].Time;

        var baselineSeries = new LineSeries<ObservablePoint>
        {
            Values = new ObservableCollection<ObservablePoint>
            {
                new(0, baseline.GetBaseline(0, dTime)),
                new(lastTime, baseline.GetBaseline(lastTime, dTime))
            },
            Stroke = new SolidColorPaint(SKColors.Gray, 2),
            Fill = null,
            GeometryFill = null,
            GeometryStroke = null,
            ZIndex = 1,
            LineSmoothness = 0,
            Name = "BaseLine",
            Tag = "Baseline"
        };

        seriesCollection.Add(baselineSeries);
    }

    public void RemoveBaseline(ObservableCollection<ISeries> seriesCollection)
    {
        var existingBaseline = seriesCollection.FirstOrDefault(s => (string)s.Tag == "Baseline");
        if (existingBaseline != null) seriesCollection.Remove(existingBaseline);
    }
}