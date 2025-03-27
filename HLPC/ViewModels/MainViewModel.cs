using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ReactiveUI;
using HLPC.Views;
using DataSet = HLPC.Models.DataSet;

namespace HLPC.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Variables
        private UserControl _currentPage;
        public UserControl CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }
        
        public static FilePickerFileType FileTypes { get; } = new(".txt and .csv")
        {
            Patterns = new[] { "*.txt", "*.csv" }
        };

        private DataSet _dataSet;
        
        public DataSet DataSet
        {
            get => _dataSet;
            set
            {
                if (_dataSet != value)
                {
                    _dataSet = value;
                    OnPropertyChanged(nameof(DataSet));
                }
            }
        }
        
        public List<DataSet> AllDataSets { get; }
        public List<DataSet> RecentDataSets { get; }
        
        // Button Commands
        public ICommand UploadFileCommand { get; set;}
        public ICommand NavigateCommand { get; }
        
        public MainViewModel()
        {
            // Change this so it requests the latest 5 from db
            AllDataSets = new List<DataSet>
            {
                new DataSet { ID = 1, Name = "Dataset 1", Date_Added = DateTime.Now.AddDays(-2) },
                new DataSet { ID = 2, Name = "Dataset 2", Date_Added = DateTime.Now.AddDays(-3) },
                new DataSet { ID = 3, Name = "Dataset 3", Date_Added = DateTime.Now.AddDays(-4) },
                new DataSet { ID = 4, Name = "Dataset 4", Date_Added = DateTime.Now.AddDays(-5) },
                new DataSet { ID = 5, Name = "Dataset 5", Date_Added = DateTime.Now.AddDays(-6) },
                new DataSet { ID = 6, Name = "Dataset 6", Date_Added = DateTime.Now.AddDays(-7) },
                new DataSet { ID = 7, Name = "Dataset 7", Date_Added = DateTime.Now.AddDays(-8) },
                new DataSet { ID = 8, Name = "Dataset 8", Date_Added = DateTime.Now.AddDays(-9) },
                new DataSet { ID = 9, Name = "Dataset 9", Date_Added = DateTime.Now.AddDays(-10) },
                new DataSet { ID = 10, Name = "Dataset 10", Date_Added = DateTime.Now.AddDays(-11) },
                new DataSet { ID = 11, Name = "Dataset 11", Date_Added = DateTime.Now.AddDays(-12) },
                new DataSet { ID = 12, Name = "Dataset 12", Date_Added = DateTime.Now.AddDays(-13) },
                new DataSet { ID = 13, Name = "Dataset 13", Date_Added = DateTime.Now.AddDays(-14) },
                new DataSet { ID = 14, Name = "Dataset 14", Date_Added = DateTime.Now.AddDays(-15) },
                new DataSet { ID = 15, Name = "Dataset 15", Date_Added = DateTime.Now.AddDays(-16) },
                new DataSet { ID = 16, Name = "Dataset 16", Date_Added = DateTime.Now.AddDays(-17) },
                new DataSet { ID = 17, Name = "Dataset 17", Date_Added = DateTime.Now.AddDays(-18) },
                new DataSet { ID = 18, Name = "Dataset 18", Date_Added = DateTime.Now.AddDays(-19) },
                new DataSet { ID = 19, Name = "Dataset 19", Date_Added = DateTime.Now.AddDays(-20) },
                new DataSet { ID = 20, Name = "Dataset 20", Date_Added = DateTime.Now.AddDays(-21) },
                new DataSet { ID = 21, Name = "Dataset 21", Date_Added = DateTime.Now.AddDays(-22) }
            };
            
            RecentDataSets = AllDataSets.Take(5).ToList();
            
            UploadFileCommand = ReactiveCommand.CreateFromTask(UploadFileAsync);
            NavigateCommand = ReactiveCommand.Create<object>(NavigateToPage);
            
            // Set default page to home
            CurrentPage = new HomeWindow(this);
        }

        public void MainViewModel2()
        {
            UploadFileCommand = ReactiveCommand.CreateFromTask(UploadFileAsync);
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

        private void NavigateToPage(object page)
        {
            if (page is string pageName)
            {
                switch (pageName)
                {
                    case "Home":
                    {
                        CurrentPage = new HomeWindow(this);
                        break;
                    }
                    case "Graph":
                    {
                        CurrentPage = new GraphWindow(this);
                        break;
                    }
                }
            }
        }
        
        // Method to set DataSet in MainViewModel from GraphWindow
        public void SetDataSet(DataSet dataSet)
        {
            DataSet = dataSet;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}