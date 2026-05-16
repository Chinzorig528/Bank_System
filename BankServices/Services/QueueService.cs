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
    public async Task<CustomerQueue> CreateQueueAsync()
    {
        var lastQueue = await _repo.GetLastQueueAsync();

        int nextNumber = 1;

        if (lastQueue != null)
        {
            string numberPart =
                lastQueue.Number.Substring(1);

            nextNumber =
                int.Parse(numberPart) + 1;
        }

        var queue = new CustomerQueue
        {
            Number = $"A{nextNumber:D3}",
            CreatedAt = DateTime.Now,
            IsCalled = false
        };

        await _repo.AddAsync(queue);

        await _repo.SaveChangesAsync();

        return queue;
    }
}