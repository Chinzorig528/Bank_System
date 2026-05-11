using BankDomain.Entities;
using BankInfrastructure.Data;
using BankInfrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankInfrastructure.Repositories;

public class QueueRepository : IQueueRepository
{
    private readonly BankDbContext _db;

    public QueueRepository(BankDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(CustomerQueue queue)
    {
        await _db.CustomerQueues.AddAsync(queue);
    }

    public async Task<List<CustomerQueue>> GetAllAsync()
    {
        return await _db.CustomerQueues.ToListAsync();
    }

    public async Task<CustomerQueue?> GetNextAsync()
    {
        return await _db.CustomerQueues
            .Where(x => !x.IsCalled)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}