using Bank.Domain.Entities;
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

    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();

    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Customer> Customers { get; set; }
}