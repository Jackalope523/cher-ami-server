using Microsoft.EntityFrameworkCore;

namespace CherAmiAPI.Contexts
{
    internal class AzureSQLStagingContext : ApplicationDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "Server=tcp:sql-cherami-staging.database.windows.net,1433;Initial Catalog=sqldb-cherami-staging;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}