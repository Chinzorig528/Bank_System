using Bank.Domain.Entities;
using BankDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankInfrastructure.Data;

/// <summary>
/// Банкны системийн Entity Framework өгөгдлийн сангийн үндсэн context.
/// </summary>
public class BankDbContext : DbContext
{
    /// <summary>
    /// Context-ийн тохиргоог гаднаас авч үүсгэнэ.
    /// </summary>
    /// <param name="options">Өгөгдлийн сангийн холболт болон EF тохиргоо.</param>
    public BankDbContext(DbContextOptions<BankDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Үйлчлүүлэгчийн queue дугааруудын хүснэгт.
    /// </summary>
    public DbSet<CustomerQueue> CustomerQueues => Set<CustomerQueue>();

    /// <summary>
    /// Валютын ханшийн хүснэгт.
    /// </summary>
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();

    /// <summary>
    /// Банкны дансны хүснэгт.
    /// </summary>
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    /// <summary>
    /// Харилцагчдын мэдээллийн хүснэгт.
    /// </summary>
    public DbSet<Customer> Customers { get; set; }
}
