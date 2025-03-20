using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace HLPC.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public ICommand UploadFileCommand { get; }
        public string uploadFile { get; } = "Upload file";

        public MainViewModel()
        {
            UploadFileCommand = ReactiveCommand.CreateFromTask(UploadFileAsync);
        }

        private async Task UploadFileAsync()
        {
            var topLevel = TopLevel.GetTopLevel(Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);
            if (topLevel == null) return;

            var documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var suggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(documentsFolderPath);

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                SuggestedStartLocation = suggestedStartLocation,
                Title = "Open Text File",
                AllowMultiple = false,
                FileTypeFilter = GetFileTypeList(),
            });

            var file = files.FirstOrDefault();
            if (file == null) return;

            await using var stream = await file.OpenReadAsync();
            using var streamReader = new StreamReader(stream);

            var fileContent = await streamReader.ReadToEndAsync();
            // Add code here to process the content of the file.
        }

        public List<FilePickerFileType> GetFileTypeList()
        {
            //list wont initialise in constructor
            List <FilePickerFileType> fileTypeList = new List<FilePickerFileType>();
            fileTypeList.Add(new FilePickerFileType(".txt and .csv"){ Patterns = new[] { "*.txt", "*.csv" } } );
            return fileTypeList;
        }
    }
}