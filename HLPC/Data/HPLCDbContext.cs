using Microsoft.EntityFrameworkCore;
using HLPC.Models;

using HLPC.Models;

namespace HLPC.Data
{
    public class HplcDbContext : DbContext
    {
        // Constructor accepts DbContextOptions to allow dependency injection (optional)
        public HplcDbContext(DbContextOptions<HplcDbContext> options) : base(options)
        {
        }

        // Define the Products DbSet
        public DbSet<Product> Products { get; set; }

        // Override OnConfiguring if not using Dependency Injection (DI)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Set the connection string here
                optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=postgres;User Id=postgres;Password=Miella-61!;");
            }
        }
    }
}
