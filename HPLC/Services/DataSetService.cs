using System;
using System.Linq;
using HPLC.Data;
using HPLC.Models;
using ReactiveUI;

namespace HPLC.Services;

public class DataSetService (SimpleKeyCRUDService<DataSet> dataSetService, HPLCDbContext context) : ReactiveObject
{
    private DataSet _selectedDataSet;
    public DataSet SelectedDataSet
    {
        get => _selectedDataSet;
        set => this.RaiseAndSetIfChanged(ref _selectedDataSet, value);
    }

    private DataSet _selectedReferenceDataSet;
    public DataSet SelectedReferenceDataSet
    {
        get => _selectedReferenceDataSet;
        set => this.RaiseAndSetIfChanged(ref _selectedReferenceDataSet, value);
    }

    public int GetLastInsertId()
    {
        return context.DataSet.OrderByDescending(e => e.ID)
            .Select(e => e.ID)
            .FirstOrDefault();
    }

    public void DeleteDataSet(int datasetId)
    {
        dataSetService.Delete(datasetId);
    }
    
    public void SetActiveDataSet(int dataSetId)
    {
        SelectedDataSet = dataSetService.GetWithChildren(dataSetId);
        SelectedDataSet.Last_Used = DateTime.Now;
        context.SaveChanges();
    }

    public void SetReferenceDataSet(int dataSetId)
    {
        SelectedReferenceDataSet = dataSetService.GetWithChildren(dataSetId);
        SelectedReferenceDataSet.Last_Used = DateTime.Now;
        context.SaveChanges();
    }
}