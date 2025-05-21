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
    private readonly ErrorService _errorSerice;

    public FileService(SimpleKeyCRUDService<DataSet> dataSetService, MessengerService messenger, ErrorService errorService)
    {
        _dataSetService = dataSetService;
        _messengerService = messenger;
        _errorSerice = errorService;
    }
    
    public async Task UploadFileAsync(string dataSetType)
    {
        var topLevel =
            TopLevel.GetTopLevel(
                Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null);
        if (topLevel == null) return;

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
        if (file == null) return;

        await using var stream = await file.OpenReadAsync();
        using var streamReader = new StreamReader(stream);

        var fileContent = await streamReader.ReadToEndAsync();
        
        if (ReadFile(file.Name,fileContent))
            _messengerService.SendMessage(dataSetType);
        else
            _messengerService.SendMessage("nee");
    }
    
    public bool ReadFile(string fileName, string fileContent)
    {
        try
        {   
            var acquiredData = fileContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(line => line.TrimStart().StartsWith("Acquired", StringComparison.OrdinalIgnoreCase));

            string result = acquiredData?.Split('\t').Last().Trim();
            string type = (!string.IsNullOrEmpty(result)) ? "Shimadzu" : "Jasco";
            DateTime sampleDate = (type=="Shimadzu")? DateTime.ParseExact(result, "d-M-yyyy HH:mm:ss", CultureInfo.InvariantCulture):new DateTime(2000, 1, 1);
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
        catch
        {
            _errorSerice.CreateWindow("Invalid file content");
            return false;
        }
    }
    
    private List<DataPoint> FormatFileContent (string fileContent, string type)
    {
        var dataPoints = new List<DataPoint>();
        var lines = fileContent.ReplaceLineEndings("\n").Split('\n');
        var valueDivider = (type=="Shimadzu")?1000:1;
        foreach (var line in lines)
        {
            var formattedLine = (Regex.Replace(line.Trim(), @"[\t; ]+", " ").Replace(",", ".")).Split(' ');

            if (formattedLine.Length == 2)
            {
                dataPoints.Add(new DataPoint()
                {
                    Time = double.Parse(formattedLine[0], CultureInfo.InvariantCulture),
                    Value = double.Parse(formattedLine[1], CultureInfo.InvariantCulture)/valueDivider
                });
            }
        }
        
        return dataPoints;
    }
}