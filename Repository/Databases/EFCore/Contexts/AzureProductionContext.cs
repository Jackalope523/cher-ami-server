using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal class AzureProductionContext : CanaryContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "Server=tcp:sparrow-stores.database.windows.net,1433;Initial Catalog=CanaryProduction;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";
            optionsBuilder.UseSqlServer(connectionString, x => x.
                UseNetTopologySuite().
                MigrationsHistoryTable("__ProductionMigrationsHistory").
                EnableRetryOnFailure());

            optionsBuilder.UseExceptionProcessor();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
