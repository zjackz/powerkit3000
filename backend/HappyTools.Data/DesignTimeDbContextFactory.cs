using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HappyTools.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            // This connection string is for design-time purposes only.
            // The actual connection string will be taken from appsettings.json at runtime.
            optionsBuilder.UseNpgsql("Host=localhost;Database=amazontrends;Username=postgres;Password=postgres123");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
