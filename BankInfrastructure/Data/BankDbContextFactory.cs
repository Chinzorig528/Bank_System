using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankInfrastructure.Data
{
    /// <summary>
    /// EF migration болон design-time командууд ажиллах үед BankDbContext үүсгэх factory.
    /// </summary>
    public class BankDbContextFactory
        : IDesignTimeDbContextFactory<BankDbContext>
    {
        /// <summary>
        /// SQLite холболтын тохиргоотой BankDbContext үүсгэнэ.
        /// </summary>
        /// <param name="args">Design-time командаас дамжих нэмэлт параметрүүд.</param>
        /// <returns>Migration хийхэд ашиглах BankDbContext.</returns>
        public BankDbContext CreateDbContext(
            string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<BankDbContext>();

            optionsBuilder.UseSqlite(
                "Data Source=bank.db");

            return new BankDbContext(
                optionsBuilder.Options);
        }
    }
}
