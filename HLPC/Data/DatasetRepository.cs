using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HLPC.Models;
using Microsoft.EntityFrameworkCore;
using SkiaSharp.HarfBuzz;

namespace HLPC.Data;

public class DatasetRepository
{
    public DatasetRepository()
    {
        
    }
    public void ReadFile(string fileName,string fileContents)
    {
        string datapointString = fileContents.Substring(fileContents.ToLower().LastIndexOf("intensity", StringComparison.Ordinal)+9);
        
        using (var dbContext = new HplcDbContext())
        {
            dbContext.Database.EnsureCreated();
            
            dbContext.DataSet.Add(new DataSet()
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                Date_Added = DateTime.Now.AddDays(-1).ToUniversalTime()
            });
            dbContext.SaveChanges();
            
            int lastSetId = dbContext.DataSet.OrderBy(x=>x.ID).Last().ID;
            List<DataPoint> dataPoints = FormatFileContents(lastSetId, datapointString);
            foreach (DataPoint dataPoint in dataPoints)
            {
              Debug.WriteLine(dataPoint);
              dbContext.DataPoints.Add(dataPoint);
            }
            dbContext.DataSet.Find(lastSetId).DataPoints.AddRange(dataPoints);
            
            dbContext.SaveChanges();
        }
    }

    private List<DataPoint> FormatFileContents(int id, string fileContents)
    {
        List<DataPoint> dataPointsList = new List<DataPoint>();
        string[] lines = fileContents.ReplaceLineEndings("\n").Split('\n');
        foreach (string line in lines)
        {
            string[] formatedLine = (Regex.Replace(line.Trim(), @"[\t; ]+", " ").Replace(",", ".")).Split(' ');
            if (formatedLine.Length == 2)
            {
                dataPointsList.Add(new DataPoint()
                {
                    DataSetID = id,
                    Time = double.Parse(formatedLine[0], CultureInfo.InvariantCulture),
                    Value = double.Parse(formatedLine[1], CultureInfo.InvariantCulture)
                });
            }
        }
            
        return dataPointsList;
    }
    public List<DataSet> GetDataset()
    {
        using (var dbContext = new HplcDbContext())
        {
            dbContext.Database.EnsureCreated();
            return dbContext.DataSet.ToList();
        }
    }
}