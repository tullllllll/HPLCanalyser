using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using HPLC.Models;
using HPLC.ViewModels;
using HPLC.Views;
using LiveChartsCore.Kernel;
using Microsoft.Extensions.DependencyInjection;

namespace HPLC.Services;

public class FileService
{
    private static FilePickerFileType FileTypes { get; } = new(".txt and .csv") {
        Patterns = ["*.txt", "*.csv"]
    };
    
    private readonly SimpleKeyCRUDService<DataSet> _dataSetService;
    private readonly MessengerService _messengerService;

    public FileService(SimpleKeyCRUDService<DataSet> dataSetService, MessengerService messenger)
    {
        _dataSetService = dataSetService;
        _messengerService = messenger;
    }
    
    public async Task<bool> UploadFileAsync(string dataSetType)
    {
        var topLevel =
            TopLevel.GetTopLevel(
                Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null);
        if (topLevel == null) return false;

        var documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var suggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(documentsFolderPath);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            SuggestedStartLocation = suggestedStartLocation,
            Title = "Open Text File",
            AllowMultiple = false,
            FileTypeFilter = new[] { FileTypes },
        });

        var file = files.FirstOrDefault();
        if (file == null) return false;

        await using var stream = await file.OpenReadAsync();
        using var streamReader = new StreamReader(stream);

        var fileContent = await streamReader.ReadToEndAsync();

        if (ReadFile(file.Name, fileContent))
        {
            _messengerService.SendMessage(dataSetType);
            return true;
        }
        
        _messengerService.SendMessage("nee");
        return false;
    }
    
    public bool ReadFile(string fileName, string fileContent)
    {
        try
        {   
            var acquiredData = fileContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(line => line.TrimStart().StartsWith("Acquired", StringComparison.OrdinalIgnoreCase));

            string? result = acquiredData?.Split('\t').Last().Trim();
            
            string type;
            DateTime sampleDate;
            
            if (string.IsNullOrEmpty(result))
            {
                type = "Jasco";
                sampleDate = new DateTime(2000, 1, 1);
            }
            else
            {
                type = "Shimadzu";
                if (DateTime.TryParse(result, CultureInfo.InvariantCulture, DateTimeStyles.None, out sampleDate))
                {
                }
                else
                {
                    ErrorService.CreateWindow("Invalid acquired date format");
                    return false;
                }
            }
            
            string datapointString = fileContent.Substring(fileContent.ToLower().LastIndexOf("intensity", StringComparison.Ordinal) + 9);
            var dataPoints = FormatFileContent(datapointString,type);

            _dataSetService.Add(new DataSet()
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                Date_Added = DateTime.Now,
                DataPoints = dataPoints,
                Last_Used = DateTime.Now,
                Sample_Date = sampleDate
            });

            return true;
        }
        catch (Exception e)
        {
            ErrorService.CreateWindow(e.Message);
            return false;
        }
    }
    
    private List<DataPoint> FormatFileContent (string fileContent, string type)
    {
        var dataPoints = new List<DataPoint>();
        var lines = fileContent.ReplaceLineEndings("\n").Split('\n');
        foreach (var line in lines)
        {
            var formattedLine = (Regex.Replace(line.Trim(), @"[\t; ]+", " ").Replace(",", ".")).Split(' ');

            if (formattedLine.Length == 2)
            {
                dataPoints.Add(new DataPoint()
                {
                    Time = double.Parse(formattedLine[0], CultureInfo.InvariantCulture),
                    Value = double.Parse(formattedLine[1], CultureInfo.InvariantCulture)
                });
            }
        }
        
        return dataPoints;
    }
}