using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using HPLC.Models;
using HPLC.Services;
using ReactiveUI;

namespace HPLC.ViewModels;

public class FileSelectViewModel
{
    private Window? _window;
    private Action? _onDatasetSelected;
    
    public event PropertyChangedEventHandler PropertyChanged;

    private ObservableCollection<DataSet> _dataSets;
    public ObservableCollection<DataSet> dataSets 
    {
        get => _dataSets;
        set
        {
            _dataSets = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(dataSets)));
        }
    }
    public string ActiveDataSetType { get; set; }
    public ICommand UploadFileCommand {get; set;}
    public ICommand SelectDatasetCommand {get; set;}
    public ICommand DeleteCommand {get; set;}
    
    // Services
    private readonly SimpleKeyCRUDService<DataSet> _dataSetCrudService;
    private readonly DataSetService _dataSetService;
    private readonly FileService _fileService;

    public FileSelectViewModel(SimpleKeyCRUDService<DataSet> dataSetCrudService, DataSetService dataSetService, FileService fileService)
    {
        _dataSetCrudService = dataSetCrudService;
        _dataSetService = dataSetService;
        _fileService = fileService;
        
        dataSets = new ObservableCollection<DataSet>(_dataSetCrudService.Get().ToList());

        UploadFileCommand = ReactiveCommand.CreateFromTask<string>(UploadFileAsync);
        SelectDatasetCommand = new RelayCommand<int>(OnSelectDataset);
        DeleteCommand = ReactiveCommand.Create<int>(DeleteDataSet);
    }
    
    public void SetHostWindow(Window window)
    {
        _window = window;
    }
    
    public void SetOnDatasetSelected(Action callback)
    {
        _onDatasetSelected = callback;
    }

    private void OnSelectDataset(int datasetId)
    {
        switch (ActiveDataSetType)
        {
            case "main":
                _dataSetService.SetActiveDataSet(datasetId);
                break;
            case "reference":
                _dataSetService.SetReferenceDataSet(datasetId);
                break;
        }
        
        _onDatasetSelected?.Invoke();
        _window?.Close();
    }

    private async Task UploadFileAsync(string dataSetType)
    {
        var result = await _fileService.UploadFileAsync(dataSetType);
        
        if (result)
            dataSets.Add(_dataSetCrudService.Get(_dataSetService.GetLastInsertId()));
    }

    private void DeleteDataSet(int id)
    {
        var item = dataSets.FirstOrDefault(x => x.ID == id);
        if (item != null)
        {
            dataSets.Remove(item); // will trigger UI update
            _dataSetService.DeleteDataSet(id);
        }
    }
}