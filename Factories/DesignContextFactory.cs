using CherAmiAPI.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace CherAmiAPI.Factories
{
    public class DesignContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        //public ApplicationDbContext CreateDbContext(string[] args)
        //{
        //    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        //    optionsBuilder.UseSqlite("Data Source=dev.db");

        //    return new ApplicationDbContext(optionsBuilder.Options);
        //}

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // JACKALOPE: Use config
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=tcp:sql-cherami-prod.database.windows.net,1433;Initial Catalog=sqldb-data-prod;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
