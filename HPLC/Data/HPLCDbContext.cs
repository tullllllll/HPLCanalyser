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
            // Potential issue when compiling: Routes outside of the debug folder
            var projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            var dbPath = Path.Combine(projectRoot, "HPLC.db");

            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}
