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
    public class SomeViewModel : ReactiveObject
    {
        public ICommand UploadFileCommand { get; }
        public string uploadFile { get; } = "Upload file";

        public SomeViewModel()
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
                FileTypeFilter = GetFileTypeList()
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
            
            
            return new List<FilePickerFileType>
            {
                new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt" } },
                new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } }
            };
        }
    }
}