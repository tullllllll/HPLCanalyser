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
            try
            {
                var options = new DbContextOptionsBuilder<HplcDbContext>()
                    .UseNpgsql("Host=127.0.0.1;Port=5432;Database=postgres;User Id=postgres;Password=Miella-61!;")
                    .Options;

                DbContext = new HplcDbContext(options);
                DbContext.Database.EnsureCreated(); // Ensures DB exists
                DbContext.Products.Add(new Product(){Id = 1,Name = "Joehoe", Price = 0.99m});
                DbContext.SaveChanges();
                Debug.WriteLine("Database setup complete!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database setup failed: {ex.Message}");
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