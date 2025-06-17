using System;
using System.Diagnostics;
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
            // START <<<<<<<<<< LINUX >>>>>>>>>>>
            // string appName = "HPLC";
            // string dbFileName = "HPLC.db"; // replace with your real name
            //
            // // Path inside AppImage (read-only, where it was bundled)
            // string sourcePath = Path.Combine(AppContext.BaseDirectory, dbFileName);
            //
            // // Writable user path (e.g., ~/.config/HPLC/HPLC.db)
            // string userDataDir = Path.Combine(
            //     Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            //     appName
            // );
            // Directory.CreateDirectory(userDataDir);
            // string dbPath = Path.Combine(userDataDir, dbFileName);
            //
            // // Copy if not exists
            // if (!File.Exists(dbPath))
            // {
            //     File.Copy(sourcePath, dbPath);
            // }
            // END <<<<<<<<<<<< LINUx >>>>>>>>>>>

            // start <<<<<<<< WINDOWS >>>>>>>>
            string dbPath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "HPLC.db");
            // end <<<<<<<<< WINDOWS >>>>>>>>>
            
            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}
