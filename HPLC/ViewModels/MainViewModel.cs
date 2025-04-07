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
using HPLC.Models;
using HPLC.Services;
using HPLC.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HPLC.ViewModels
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
        
        private static FilePickerFileType FileTypes { get; } = new(".txt and .csv") {
            Patterns = ["*.txt", "*.csv"]
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
        
        public IEnumerable<DataSet> RecentDataSets { get; set; }
        
        // Button Commands
        public ICommand UploadFileCommand { get; set; }
        public ICommand NavigateCommand { get; set;  }
        
        // Services
        private readonly SimpleKeyCRUDService<DataSet> _dataSetCrudService;
        private readonly DataSetService _dataSetService;
        private readonly IServiceProvider _serviceProvider;
        
        public MainViewModel(SimpleKeyCRUDService<DataSet> dataSetCrudService, DataSetService dataSetService, IServiceProvider serviceProvider)
        {
            // for Dependency injection
            _dataSetCrudService = dataSetCrudService;
            _dataSetService = dataSetService;
            _serviceProvider = serviceProvider;
            
            // Set variables
            RecentDataSets = _dataSetCrudService.Get().ToList().Take(5);
            _dataSet = _dataSetCrudService.GetWithChildren(1);
            
            // Button Commands
            UploadFileCommand = ReactiveCommand.CreateFromTask(UploadFileAsync);
            NavigateCommand = ReactiveCommand.Create<object>(NavigateToPage);
            
            // Set default page to home
            CurrentPage = _serviceProvider.GetRequiredService<HomeWindow>();
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
            
            _dataSetService.ReadFile(file.Name,fileContent);
            CurrentPage = _serviceProvider.GetRequiredService<HomeWindow>();
        }

        private void NavigateToPage(object page)
        {
            if (page is string pageName)
            {
                CurrentPage = pageName switch
                {
                    "Home" => _serviceProvider.GetRequiredService<HomeWindow>(),
                    "Graph" => _serviceProvider.GetRequiredService<GraphWindow>(),
                    _ => CurrentPage
                };
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}