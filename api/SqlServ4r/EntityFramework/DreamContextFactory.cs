using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SqlServ4r.EntityFramework
{
    public class DreamContextFactory : IDesignTimeDbContextFactory<DreamContext>
    {
        public DreamContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("MyApp")
                ?? "Host=localhost;Port=5432;Database=seafood_crm;Username=seafood;Password=seafood";

            var optionsBuilder = new DbContextOptionsBuilder<DreamContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new DreamContext(optionsBuilder.Options);
        }
    }
}
