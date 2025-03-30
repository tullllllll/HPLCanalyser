using Microsoft.EntityFrameworkCore;
using HLPC.Models;
using SQLitePCL;



namespace HLPC.Data
{
    public class HplcDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<DataSet> DataSet { get; set; } = null!;
        public DbSet<DataPoint> DataPoints { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Batteries.Init();
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }
}
