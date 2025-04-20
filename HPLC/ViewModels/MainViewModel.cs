using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        // ViewModels
        public GraphViewModel GraphViewModel { get; set; }
        
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
        public DataSet DataSet
        {
            get => _dataSetService.SelectedDataSet;
            set
            {
                if (_dataSetService.SelectedDataSet != value)
                {
                    _dataSetService.SelectedDataSet = value;
                    OnPropertyChanged(nameof(DataSet));
                }
            }
        }

        public DataSet ReferenceDataSet
        {
            get => _dataSetService.SelectedReferenceDataSet;
            set
            {
                if (_dataSetService.SelectedReferenceDataSet != value)
                {
                    _dataSetService.SelectedReferenceDataSet = value;
                    OnPropertyChanged(nameof(ReferenceDataSet));
                }
            }
        }
        public ObservableCollection<Peak> Peaks { get; set; } = new ObservableCollection<Peak>();
        public ObservableCollection<Peak> ReferencePeaks { get; set; } = new ObservableCollection<Peak>();


        public void LoadPeaks(List<DataPoint> dataPoints, double threshold, double minPeakWidth)
        {
            var mathService = new MathService();
            var detectedPeaks = mathService.DetectPeaks(dataPoints, threshold, minPeakWidth);
            Peaks.Clear();
            foreach (var peak in detectedPeaks)
            {
                Peaks.Add(peak);
            }
        }
        
        public void LoadReferencePeaks(List<DataPoint> dataPoints, double threshold, double minPeakWidth)
        {
            var mathService = new MathService();
            var detectedPeaks = mathService.DetectPeaks(dataPoints, threshold, minPeakWidth);
            ReferencePeaks.Clear();
            foreach (var peak in detectedPeaks)
            {
                ReferencePeaks.Add(peak);
            }
        }
        
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
            
            // Set viewmodels
            GraphViewModel = _serviceProvider.GetService<GraphViewModel>();
            
            // Set variables
            DataSet = _dataSetCrudService.GetWithChildren(1);
            
            // Button Commands
            UploadFileCommand = ReactiveCommand.CreateFromTask<string>(UploadFileAsync);
            NavigateCommand = ReactiveCommand.Create<object>(NavigateToPage);
            
            // Set default page to home
            CurrentPage = _serviceProvider.GetRequiredService<HomeWindow>();
        }

        private async Task UploadFileAsync(string dataSetType)
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
            switch (dataSetType)
            {
                case "reference":
                {
                    ReferenceDataSet = _dataSetCrudService.GetWithChildren(_dataSetCrudService.Get().ToList().Count());
                    LoadReferencePeaks(ReferenceDataSet.DataPoints.ToList(),0,0);
                    break;
                }
                case "main":
                {
                    DataSet = _dataSetCrudService.GetWithChildren(_dataSetCrudService.Get().ToList().Count());
                    LoadPeaks(DataSet.DataPoints.ToList(), 0, 0);
                    break;
                }
            }

            if (CurrentPage is not GraphWindow)
            {
                CurrentPage = null;
                CurrentPage = _serviceProvider.GetRequiredService<GraphWindow>();
            }
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