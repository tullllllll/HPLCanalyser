using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using HPLC.Models;
using HPLC.ViewModels;
using HPLC.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HPLC.Services;

public class FileService
{
    private static FilePickerFileType FileTypes { get; } = new(".txt and .csv") {
        Patterns = ["*.txt", "*.csv"]
    };
    
    private readonly DataSetService _dataSetService;
    private readonly MessengerService _messengerService;

    public FileService(DataSetService dataSetService, MessengerService messenger)
    {
        _dataSetService = dataSetService;
        _messengerService = messenger;
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
        
        _dataSetService.ReadFile(file.Name,fileContent);
        
        _messengerService.SendMessage(dataSetType);
    }
}