using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using HLPC.ViewModels;
using HLPC.Views;

namespace HLPC.Views
{

    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }


    }
}
