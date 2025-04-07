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
        // Main View Model
        collection.AddTransient<MainViewModel>();
        
        // Views
        collection.AddSingleton<HomeWindow>();
        collection.AddSingleton<GraphWindow>();
        
        // Database Context
        collection.AddSingleton<HPLCDbContext>();
        
        // CRUD Services
        collection.AddScoped<SimpleKeyCRUDService<DataSet>>();
        
        // Services
        collection.AddScoped<DataSetService>();
    } 
}