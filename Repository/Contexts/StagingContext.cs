using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Repository.Contexts
{
    internal class StagingContext : CardinalContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "Host=localhost;Port=5432;Database=cardinal-test;Username=postgres;Password=SneakyPuma5233!!";

            optionsBuilder.UseSqlServer(connectionString, x => x.
                MigrationsHistoryTable("__StagingMigrationsHistory").
                EnableRetryOnFailure());

            optionsBuilder.UseExceptionProcessor();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
