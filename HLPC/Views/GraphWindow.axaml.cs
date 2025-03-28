using System;
using System.Collections.Generic;
using Avalonia.Controls;
using HLPC.Models;
using HLPC.ViewModels;
using System.Windows.Input;
using ReactiveUI;

namespace HLPC.Views;

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
            Variables = new List<Variable>
            {
                new Variable{ ID = 1, DataSetID = 1, Type = "string", Value = "Some Value 1"},
                new Variable{ ID = 2, DataSetID = 1, Type = "string", Value = "Some Value 2"},
                new Variable{ ID = 3, DataSetID = 1, Type = "string", Value = "Some Value 3"},
                new Variable{ ID = 4, DataSetID = 1, Type = "string", Value = "Some Value 4"},
                new Variable{ ID = 5, DataSetID = 1, Type = "string", Value = "Some Value 5"},
                new Variable{ ID = 6, DataSetID = 1, Type = "string", Value = "Some Value 6"},
                new Variable{ ID = 7, DataSetID = 1, Type = "string", Value = "Some Value 7"},
                new Variable{ ID = 8, DataSetID = 1, Type = "string", Value = "Some Value 8"},
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
            Variables = new List<Variable>
            {
                new Variable{ ID = 1, DataSetID = 1, Type = "string", Value = "Some Value 1"},
                new Variable{ ID = 2, DataSetID = 1, Type = "string", Value = "Some Value 2"},
                new Variable{ ID = 3, DataSetID = 1, Type = "string", Value = "Some Value 3"},
            }};
        
        this.DataContext = this;
    }

    public void NavigateToPage(object page)
    {
        ViewModel.NavigateCommand.Execute(page);
    }
}