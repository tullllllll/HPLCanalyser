using Microsoft.EntityFrameworkCore;
using HLPC.Models;
using SQLitePCL;



namespace HLPC.Data
{
    public class HplcDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Batteries.Init();
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }
}
