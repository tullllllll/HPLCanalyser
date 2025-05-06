using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using HPLC.Models;
using HPLC.Services;
using HPLC.Views;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace HPLC.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        // ViewModels
        public GraphViewModel GraphViewModel { get; set; }
        public FileSelectViewModel FileSelectViewModel { get; set; }
        
        // Variables
        private UserControl _currentPage;
        private bool _isNavOpen;

        public bool IsNavOpen
        {
            get => _isNavOpen;
            set => this.RaiseAndSetIfChanged(ref _isNavOpen, value);
        }
        
        public string ToggleButtonContent => IsNavOpen ? "<" : ">";
        
        public UserControl CurrentPage 
        { 
            get => _currentPage; 
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }
       
        public DataSet DataSet
        {
            get => _dataSetService.SelectedDataSet;
            set
            {
                if (_dataSetService.SelectedDataSet != value)
                {
                    _dataSetService.SelectedDataSet = value;
                    this.RaisePropertyChanged(nameof(DataSet));
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
                    this.RaisePropertyChanged(nameof(ReferenceDataSet));
                }
            }
        }
        
        // Button Commands
        public ICommand NavigateCommand { get; set;  }
        public ICommand SelectFileCommand { get; set; }
        public ICommand DeselectFileCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ToggleNavCommand { get; }
        
        // Services
        private readonly SimpleKeyCRUDService<DataSet> _dataSetCrudService;
        private readonly DataSetService _dataSetService;
        private readonly MessengerService _messengerService;
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationService _navigationService;
        
        private FileSelect window { get; set; }
        
        public MainViewModel(SimpleKeyCRUDService<DataSet> dataSetCrudService, DataSetService dataSetService, MessengerService messengerService, NavigationService navigationService)
        {
            // for Dependency injection
            _dataSetCrudService = dataSetCrudService;
            _dataSetService = dataSetService;
            _messengerService = messengerService;
            _serviceProvider = App.ServiceProvider;
            _navigationService = navigationService;

            // Delegate commands
            _messengerService.FileUploaded += FileHasBeenUploaded;
            _navigationService.Navigate = NavigateToPage;
            
            // Set viewmodels
            GraphViewModel = _serviceProvider.GetService<GraphViewModel>();
            FileSelectViewModel = _serviceProvider.GetService<FileSelectViewModel>();
            
            // Button Commands
            NavigateCommand = ReactiveCommand.Create<object>(NavigateToPage);
            SelectFileCommand = ReactiveCommand.Create<string>(SelectFile);
            DeselectFileCommand = ReactiveCommand.Create<string>(DeselectFile);
            ToggleNavCommand = ReactiveCommand.Create(() =>
            {
                IsNavOpen = !IsNavOpen;
                this.RaisePropertyChanged(nameof(ToggleButtonContent));
            });
            
            // Subscribe for property changes inside dataset service
            _dataSetService.WhenAnyValue(x => x.SelectedDataSet)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(DataSet)));
            
            _dataSetService.WhenAnyValue(x => x.SelectedReferenceDataSet)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ReferenceDataSet)));
            
            // Set default page to home
            CurrentPage = _serviceProvider.GetRequiredService<HomeWindow>();
        }

        private void SelectFile(string dataSetType)
        {
            FileSelectViewModel.ActiveDataSetType = dataSetType;
            window = new FileSelect(FileSelectViewModel);
            window.Show();
        }
        private void DeselectFile(string dataSetType)
        {
            if (dataSetType=="reference") ReferenceDataSet = null;
            if (dataSetType=="main") DataSet = null;
        }

        private void FileHasBeenUploaded(string dataSetType)
        {
            switch (dataSetType)
            {
                case "reference":
                {
                    // Change to last insert
                    ReferenceDataSet = _dataSetCrudService.GetWithChildren(_dataSetService.GetLastInsertId());
                    break;
                }
                case "main":
                {
                    DataSet = _dataSetCrudService.GetWithChildren(_dataSetService.GetLastInsertId());
                    break;
                }
            }

            if (CurrentPage is not GraphWindow && dataSetType != "nee")
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

                if (window != null)
                {
                    if (window.IsVisible)
                        window.Close();
                }
            }
        }
        
        // public event PropertyChangedEventHandler PropertyChanged;
        // protected void OnPropertyChanged(string propertyName) => 
        //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}