using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using HPLC.Models;
using HPLC.Services;
using ReactiveUI;

namespace HPLC.ViewModels;

public class FileSelectViewModel
{
    public List<DataSet> dataSets
    {
        get => _dataSetService.Get().ToList();
        set{}
    }
    
    public ICommand UploadFileCommand {get; set;}
    
    // Services
    private readonly SimpleKeyCRUDService<DataSet> _dataSetService;
    private readonly FileService _fileService;

    public FileSelectViewModel(SimpleKeyCRUDService<DataSet> dataSetService, FileService fileService)
    {
        _dataSetService = dataSetService;
        _fileService = fileService;

        UploadFileCommand = ReactiveCommand.CreateFromTask<string>(_fileService.UploadFileAsync);
    }
}