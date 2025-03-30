using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using HLPC.Data;
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
        
        public List<DataSet> AllDataSets { get; set; }
        public List<DataSet> RecentDataSets { get; set; }
        
        // Button Commands
        public ICommand UploadFileCommand { get; set;}
        public ICommand NavigateCommand { get; }
        private DatasetRepository _datasetRepository;
        
        public MainViewModel()
        {
            _datasetRepository = new DatasetRepository();
            _datasetRepository.GetDataset();
            AllDataSets = _datasetRepository.GetDataset();
            RecentDataSets = AllDataSets.Take(5).ToList();
            UploadFileCommand = ReactiveCommand.CreateFromTask(UploadFileAsync);
            NavigateCommand = ReactiveCommand.Create<object>(NavigateToPage);
            
            // Set default page to home
            CurrentPage = new HomeWindow(this);
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
            DatasetRepository datasetRepository = new DatasetRepository();
            datasetRepository.ReadFile(file.Name,fileContent);
            CurrentPage = new GraphWindow(this);
        }

        private void NavigateToPage(object page)
        {
            _datasetRepository.GetDataset();
            AllDataSets = _datasetRepository.GetDataset();
            if (page is string pageName)
            {
                switch (pageName)
                {
                    case "Home":
                    {
                        RecentDataSets = AllDataSets.Take(5).ToList();
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