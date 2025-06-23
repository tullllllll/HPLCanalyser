using System;
using System.Collections.Generic;
using System.Linq;
using HPLC.Models;
using HPLC.Services;

namespace HPLC;

public struct Baseline
{
    private double A { get; set; }
    private double B { get; set; }

    public Baseline(double a, double b)
    {
        A = a;
        B = b;
    }
    public static Baseline CalculateBaseline(List<DataPoint> dataPoints, double dTime, int pointsToUse)
    {
        if (dataPoints.Count < pointsToUse)
        {
            return new Baseline(0, 0); // fallback: flat line
        }

        if (pointsToUse < 3)
        {
            return new Baseline(0, 0); // fallback: flat line
        }

        int thirdSlice = (int)Math.Round((double)pointsToUse / 3, MidpointRounding.AwayFromZero);
        
        // Helper to get average point from a slice
        (double avgTime, double avgValue) Avg(int start)
        {
            var slice = dataPoints.Skip(start).Take(thirdSlice).ToList();
            double avgTime = slice.Average(dp => dp.Time/dTime);
            double avgValue = slice.Average(dp => dp.Value);
            return (avgTime, avgValue);
        }
        
        // Get the three average points
        var (x1, y1) = Avg(0);    // First 30
        var (x2, y2) = Avg(thirdSlice);   // Second 30
        var (x3, y3) = Avg(thirdSlice*2);   // Third 30

        // Fit a line using linear regression on the three points
        double[] xs = { x1, x2, x3 };
        double[] ys = { y1, y2, y3 };

        double sumX = xs.Sum();
        double sumY = ys.Sum();
        double sumXY = xs.Zip(ys, (x, y) => x * y).Sum();
        double sumX2 = xs.Sum(x => x * x);
        int n = 3;

        double denominator = n * sumX2 - sumX * sumX;
        
        double a = (n * sumXY - sumX * sumY) / denominator;
        double b = (sumY - a * sumX) / n;
        
        if (Math.Abs(denominator) < 1e-2 || Math.Abs(a) < 1e-2) return new Baseline(0, ys.Average()); // fallback: flat line
        
        return new Baseline(a, b);
    }

    public double GetBaseline(double time, double dTime)
    {
        return A * time * 1/dTime + B;
    }

    public override string ToString()
    {
        return A + " " + B;
    }
}