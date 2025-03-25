using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HLPC.Data;
using HLPC.Models;
using Microsoft.EntityFrameworkCore;

namespace HLPC
{
    public partial class App : Application
    {
        public static HplcDbContext DbContext { get; private set; }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            SetupDatabase();
        }
        private void SetupDatabase()
        {
            
            using (var db = new HplcDbContext())
            {
                //db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                Debug.WriteLine("Database setup complete!");
            }  
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow()
                {
                    WindowState = WindowState.Maximized
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}