using Microsoft.EntityFrameworkCore;

namespace CherAmiAPI.Contexts
{
    internal class PostgreSQLContext : ApplicationDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "Host=localhost;Port=5432;Database=sqldb-cherami-dev;Username=postgres;Password=SneakyPuma5233!!";
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}