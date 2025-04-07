using System;
using System.Collections.Generic;
using Avalonia.Controls;
using System.Windows.Input;
using HPLC.Models;
using HPLC.Services;
using HPLC.ViewModels;
using ReactiveUI;

namespace HPLC.Views;

public partial class GraphWindow : UserControl
{
    private DataSet _dataSet { get; set; }
    
    public MainViewModel ViewModel => (MainViewModel)DataContext;
    
    // Button commands
    public ICommand NavigateCommand { get; }
    
    public GraphWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        
        NavigateCommand = ReactiveCommand.Create<object>(NavigateToPage);
        DataContext = viewModel;
        var graphControl = new GraphUserControl(viewModel.DataSet);
        MainGraph.Child = graphControl;
    }

    public void NavigateToPage(object page)
    {
        ViewModel.NavigateCommand.Execute(page);
    }
}