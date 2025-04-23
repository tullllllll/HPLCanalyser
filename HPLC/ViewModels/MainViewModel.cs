using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
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
        public FileSelectViewModel FileSelectViewModel { get; set; }
        
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
        
        // Button Commands
        public ICommand NavigateCommand { get; set;  }
        public ICommand SelectFileCommand { get; set; }
        
        // Services
        private readonly SimpleKeyCRUDService<DataSet> _dataSetCrudService;
        private readonly DataSetService _dataSetService;
        private readonly MessengerService _messengerService;
        private readonly IServiceProvider _serviceProvider;
        
        private FileSelect window { get; set; }
        
        public MainViewModel(SimpleKeyCRUDService<DataSet> dataSetCrudService, DataSetService dataSetService, IServiceProvider serviceProvider, MessengerService messengerService)
        {
            // for Dependency injection
            _dataSetCrudService = dataSetCrudService;
            _dataSetService = dataSetService;
            _messengerService = messengerService;
            _serviceProvider = serviceProvider;

            _messengerService.FileUploaded += FileHasBeenUploaded;
            
            // Set viewmodels
            GraphViewModel = _serviceProvider.GetService<GraphViewModel>();
            FileSelectViewModel = _serviceProvider.GetService<FileSelectViewModel>();
            
            // Set variables
            DataSet = _dataSetCrudService.GetWithChildren(1);
            
            // Button Commands
            NavigateCommand = ReactiveCommand.Create<object>(NavigateToPage);
            SelectFileCommand = ReactiveCommand.Create(SelectFile);
            
            // Set default page to home
            CurrentPage = _serviceProvider.GetRequiredService<HomeWindow>();
        }

        private void SelectFile()
        {
            window = new FileSelect(FileSelectViewModel);
            window.Show();
        }

        private void FileHasBeenUploaded(string dataSetType)
        {
            switch (dataSetType)
            {
                case "reference":
                {
                    // Change to last insert
                    ReferenceDataSet = _dataSetCrudService.GetWithChildren(_dataSetService.GetLastInsertID());
                    break;
                }
                case "main":
                {
                    DataSet = _dataSetCrudService.GetWithChildren(_dataSetService.GetLastInsertID());
                    break;
                }
            }

            if (CurrentPage is not GraphWindow)
            {
                CurrentPage = null;
                CurrentPage = _serviceProvider.GetRequiredService<GraphWindow>();
            }
            
            window.Close();
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