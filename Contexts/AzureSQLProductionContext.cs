using Microsoft.EntityFrameworkCore;

namespace CherAmiAPI.Contexts
{
    internal class AzureSQLProductionContext : ApplicationDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "Server=tcp:sql-cherami-prod.database.windows.net,1433;Initial Catalog=sqldb-data-prod;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}