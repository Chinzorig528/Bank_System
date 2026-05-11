using BankDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankInfrastructure.Data;

public class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options)
        : base(options)
    {
    }

    public DbSet<CustomerQueue> CustomerQueues => Set<CustomerQueue>();
}