using HPLC.Data;
using HPLC.Models;
using HPLC.Services;
using HPLC.ViewModels;
using HPLC.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HPLC;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        // ViewModels
        collection.AddSingleton<MainViewModel>();
        collection.AddSingleton<GraphViewModel>();
        collection.AddSingleton<FileSelectViewModel>();
        
        // Views
        collection.AddSingleton<GraphWindow>();
        collection.AddSingleton<GraphUserControl>();
        
        // Database Context
        collection.AddSingleton<HPLCDbContext>();
        
        // CRUD Services
        collection.AddScoped<SimpleKeyCRUDService<DataSet>>();
        
        // Services
        collection.AddScoped<DataSetService>();
        collection.AddSingleton<MessengerService>();
        collection.AddScoped<FileService>();
        collection.AddScoped<MathService>();
    } 
}