using BankDomain.Entities;
using BankInfrastructure.Interfaces;
using BankServices.Interfaces;

namespace BankServices.Services;

public class QueueService : IQueueService
{
    private readonly IQueueRepository _repo;

    private static int _counter = 1;

    public QueueService(IQueueRepository repo)
    {
        _repo = repo;
    }

    public async Task<CustomerQueue> CreateQueueAsync()
    {
        var queue = new CustomerQueue
        {
            Number = $"A{_counter:D3}",
            CreatedAt = DateTime.Now,
            IsCalled = false
        };

        _counter++;

        await _repo.AddAsync(queue);
        await _repo.SaveChangesAsync();

        return queue;
    }

    public async Task<CustomerQueue?> CallNextAsync()
    {
        var next = await _repo.GetNextAsync();

        if (next == null)
            return null;

        next.IsCalled = true;

        await _repo.SaveChangesAsync();

        return next;
    }

    public async Task<List<CustomerQueue>> GetAllAsync()
    {
        return await _repo.GetAllAsync();
    }
}