using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    
    public ObservableCollection<DataSet> dataSets { get; } = [];
    public string ActiveDataSetType { get; set; }
    public ICommand UploadFileCommand {get; set;}
    
    // Services
    private readonly SimpleKeyCRUDService<DataSet> _dataSetCrudService;
    private readonly DataSetService _dataSetService;
    private readonly FileService _fileService;

    public FileSelectViewModel(SimpleKeyCRUDService<DataSet> dataSetCrudService, DataSetService dataSetService, FileService fileService)
    {
        _dataSetCrudService = dataSetCrudService;
        _dataSetService = dataSetService;
        _fileService = fileService;
        
        foreach (var ds in _dataSetCrudService.Get())
        {
            ds.SelectCommand = new RelayCommand(() => OnSelectDataset(ds.ID));
            ds.DeleteCommand = new RelayCommand(() => DeleteDataSet(ds.ID));
            dataSets.Add(ds);
        }

        UploadFileCommand = ReactiveCommand.CreateFromTask<string>(UploadFileAsync);
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
        {
            var newSet = _dataSetCrudService.Get(_dataSetService.GetLastInsertId());
            newSet.SelectCommand = new RelayCommand(() => OnSelectDataset(newSet.ID));
            newSet.DeleteCommand = new RelayCommand(() => DeleteDataSet(newSet.ID));
            dataSets.Add(newSet);
        }
        
        _onDatasetSelected?.Invoke();
        _window?.Close();
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