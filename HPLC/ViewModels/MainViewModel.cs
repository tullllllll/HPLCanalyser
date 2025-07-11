﻿using System;
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
        public DataSet DataSet
        {
            get => _dataSetService.SelectedDataSet;
            set
            {
                if (_dataSetService.SelectedDataSet != value)
                {
                    _dataSetService.SelectedDataSet = value;
                    this.RaisePropertyChanged(nameof(DataSet));
                    this.RaisePropertyChanged(nameof(IsDatasetNull));
                }
            }
        }

        public bool IsDatasetNull => DataSet == null;

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

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                this.RaisePropertyChanged(nameof(SelectedTabIndex));
            }
        }
        
        // Button Commands
        public ICommand SelectFileCommand { get; set; }
        public ICommand DeselectFileCommand { get; set; }
        
        // Services
        private readonly SimpleKeyCRUDService<DataSet> _dataSetCrudService;
        private readonly DataSetService _dataSetService;
        private readonly MessengerService _messengerService;
        private readonly IServiceProvider _serviceProvider;
        
        private FileSelect window { get; set; }
        
        public MainViewModel(SimpleKeyCRUDService<DataSet> dataSetCrudService, DataSetService dataSetService, MessengerService messengerService)
        {
            // for Dependency injection
            _dataSetCrudService = dataSetCrudService;
            _dataSetService = dataSetService;
            _messengerService = messengerService;
            _serviceProvider = App.ServiceProvider;

            // Delegate commands
            _messengerService.FileUploaded += FileHasBeenUploaded;
            
            // Set viewmodels
            GraphViewModel = _serviceProvider.GetService<GraphViewModel>();
            FileSelectViewModel = _serviceProvider.GetService<FileSelectViewModel>();
            
            // Button Commands
            SelectFileCommand = ReactiveCommand.Create<string>(SelectFile);
            DeselectFileCommand = ReactiveCommand.Create<string>(DeselectFile);
            
            // Subscribe for property changes inside dataset service
            _dataSetService.WhenAnyValue(x => x.SelectedDataSet)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(DataSet));
                    this.RaisePropertyChanged(nameof(IsDatasetNull));
                });
            
            _dataSetService.WhenAnyValue(x => x.SelectedReferenceDataSet)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ReferenceDataSet)));
        }

        private void SelectFile(string dataSetType)
        {
            FileSelectViewModel.ActiveDataSetType = dataSetType;
            window = new FileSelect(FileSelectViewModel);
            FileSelectViewModel.SetHostWindow(window);
            
            FileSelectViewModel.SetOnDatasetSelected(() =>
            {
                SelectedTabIndex = 0;
            });
            
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
            
            window.Close();
        }
    }
}