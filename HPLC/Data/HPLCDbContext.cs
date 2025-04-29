using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using HPLC.Models;

namespace HPLC.Data
{
    public class HPLCDbContext : DbContext
    {
        public DbSet<DataSet> DataSet { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "HPLC.db");
            File.AppendAllText("app_log.txt", $"Database path: {dbPath}{Environment.NewLine}");

            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}
