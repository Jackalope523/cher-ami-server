//using CherAmiAPI.Contexts;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.Options;

//namespace CherAmiAPI.Factories
//{
//    public class DesignContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
//    {
//        //public ApplicationDbContext CreateDbContext(string[] args)
//        //{
//        //    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
//        //    optionsBuilder.UseSqlite("Data Source=dev.db");

//        //    return new ApplicationDbContext(optionsBuilder.Options);
//        //}

//        public ApplicationDbContext CreateDbContext(string[] args)
//        {
//            return new PostgreSQLContext();
//        }
//    }
//}
