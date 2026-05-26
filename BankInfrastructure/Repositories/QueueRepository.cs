using BankDomain.Entities;
using BankInfrastructure.Data;
using BankInfrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankInfrastructure.Repositories;

/// <summary>
/// Entity Framework ашиглан харилцагчийн queue бичлэгүүдийг хадгалах repository.
/// </summary>
public class QueueRepository : IQueueRepository
{
    private readonly BankDbContext _db;

    /// <summary>
    /// Queue repository-д шаардлагатай өгөгдлийн сангийн context-ийг онооно.
    /// </summary>
    /// <param name="db">Банкны өгөгдлийн сангийн context.</param>
    public QueueRepository(BankDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task AddAsync(CustomerQueue queue)
    {
        await _db.CustomerQueues.AddAsync(queue);
    }

    /// <inheritdoc />
    public async Task<List<CustomerQueue>> GetAllAsync()
    {
        return await _db.CustomerQueues.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<CustomerQueue?> GetNextAsync()
    {
        return await _db.CustomerQueues
            .Where(x => !x.IsCalled)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<CustomerQueue?> GetLastQueueAsync()
    {
        return await _db.CustomerQueues
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }
}
