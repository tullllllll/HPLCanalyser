using System;
using System.Collections.Generic;
using Avalonia.Controls;
using System.Windows.Input;
using HPLC.Models;
using HPLC.ViewModels;
using ReactiveUI;

namespace HPLC.Views;

public partial class GraphWindow : UserControl
{
    private DataSet _dataSet { get; set; }
    public DataSet DataSet { get => _dataSet; }
    
    public MainViewModel ViewModel => (MainViewModel)DataContext;
    
    // Button commands
    public ICommand NavigateCommand { get; }
    public GraphWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        NavigateCommand = ReactiveCommand.Create<object>(NavigateToPage);
        DataContext = viewModel;
        
        // placeholder for testing
        _dataSet = new DataSet { 
            ID = 1, 
            Name = "Dataset 1", 
            Date_Added = DateTime.Now.AddDays(-2), 
            DataPoints = new List<DataPoint>
            {
                new DataPoint{ ID = 1, DataSetID = 1, Time = 0.008333, Value = 3}
            }};

        viewModel.SetDataSet(_dataSet);
    }
    
    public GraphWindow(MainViewModel viewModel, int DataSetID)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // TODO: Check if DataSetID Exists in DB and if so, fetch from db
        _dataSet = new DataSet { 
            ID = 1, 
            Name = "Dataset 1", 
            Date_Added = DateTime.Now.AddDays(-2), 
            DataPoints = new List<DataPoint>
            {
                new DataPoint{ ID = 1, DataSetID = 1, Time = 0.03, Value = 6}
            }};
        
        this.DataContext = this;
    }

    public void NavigateToPage(object page)
    {
        ViewModel.NavigateCommand.Execute(page);
    }
}