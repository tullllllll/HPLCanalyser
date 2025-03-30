using Microsoft.EntityFrameworkCore;
using HLPC.Models;

using HLPC.Models;

namespace HLPC.Data
{
    public class HplcDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<DataSet> DataSet { get; set; }
        public DbSet<DataPoint> DataPoints { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=postgres;User Id=postgres;Password=****;");
                
                
            }
        }
    }
}
