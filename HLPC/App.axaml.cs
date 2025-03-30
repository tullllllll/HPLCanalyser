using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HLPC.Data;
using HLPC.Models;
using Microsoft.EntityFrameworkCore;
using HLPC.Views;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace HLPC;

public partial class App : Application
{
  

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

            db.Products.Add(new Product
            {
                Name = "Sample Product",
                Price = 9.99m
            });

            db.DataSet.Add(new DataSet
            {
                Name = "Sample DataSet",
                Date_Added = DateTime.Now,

            });
            db.SaveChanges();
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