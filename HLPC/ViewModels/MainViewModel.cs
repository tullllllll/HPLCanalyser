using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ReactiveUI;
using HLPC.Models;
using DataSet = HLPC.Models.DataSet;

namespace HLPC.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public ICommand UploadFileCommand { get; }
        public string UploadFile { get; } = "New DataSet";
        public ObservableCollection<DataSet> RecentDataSets { get; }

        public static FilePickerFileType FileTypes { get; } = new(".txt and .csv")
        {
            Patterns = new[] { "*.txt", "*.csv" }
        };

        public MainViewModel()
        {
            UploadFileCommand = ReactiveCommand.CreateFromTask(UploadFileAsync);
            RecentDataSets = new ObservableCollection<DataSet>
            {
                new DataSet { ID = 1, Name = "Dataset 1", Date_Added = DateTime.Now.AddDays(-2), Machine_Type = "Machine 1"},
                new DataSet { ID = 2, Name = "Dataset 2", Date_Added = DateTime.Now.AddDays(-3), Machine_Type = "Machine 2"},
                new DataSet { ID = 3, Name = "Dataset 3", Date_Added = DateTime.Now.AddDays(-4), Machine_Type = "Machine 2"},
                new DataSet { ID = 4, Name = "Dataset 4", Date_Added = DateTime.Now.AddDays(-5), Machine_Type = "Machine 1"},
                new DataSet { ID = 5, Name = "Dataset 5", Date_Added = DateTime.Now.AddDays(-6), Machine_Type = "Machine 2"}
            };
        }

        private async Task UploadFileAsync()
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
            // Add code here to process the content of the file.
        }
    }
}