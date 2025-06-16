using EntityFramework.Exceptions.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal class DevelopmentContext : CanaryContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string absolutePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "Repository.Tests","TestDB.db");
  
            Console.WriteLine(absolutePath);

            optionsBuilder.UseSqlite($"Data Source={absolutePath}", x => x.UseNetTopologySuite());
            optionsBuilder.UseExceptionProcessor();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
