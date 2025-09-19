using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace powerkit3000.data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            // This connection string is for design-time purposes only.
            // The actual connection string will be taken from appsettings.json at runtime.
            optionsBuilder.UseNpgsql("Host=192.168.1.120;Port=5432;Database=postgres;Username=postgres;Password=123321");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
