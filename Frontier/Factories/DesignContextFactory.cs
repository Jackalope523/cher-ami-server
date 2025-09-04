using CrazyLizard.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CrazyLizard.Factories
{
    public class DesignContextFactory : IDesignTimeDbContextFactory<CrazyLizardContext>
    {
        public CrazyLizardContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CrazyLizardContext>();
            optionsBuilder.UseSqlite("Data Source=dev.db");

            return new CrazyLizardContext(optionsBuilder.Options);
        }
    }
}
