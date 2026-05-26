using BankDomain.Entities;
using BankInfrastructure.Interfaces;
using BankServices.Interfaces;

namespace BankServices.Services;

/// <summary>
/// Queue дугаар үүсгэх болон дараагийн үйлчлүүлэгчийг дуудах бизнес логик.
/// </summary>
public class QueueService : IQueueService
{
    private readonly IQueueRepository _repo;

    /// <summary>
    /// Queue service-д хэрэгтэй repository-г онооно.
    /// </summary>
    /// <param name="repo">Queue өгөгдөлтэй ажиллах repository.</param>
    public QueueService(IQueueRepository repo)
    {
        _repo = repo;
    }

    /// <inheritdoc />
    public async Task<CustomerQueue?> CallNextAsync()
    {
        var next = await _repo.GetNextAsync();

        if (next == null)
            return null;

        next.IsCalled = true;

        await _repo.SaveChangesAsync();

        return next;
    }

    /// <inheritdoc />
    public async Task<List<CustomerQueue>> GetAllAsync()
    {
        return await _repo.GetAllAsync();
    }

    /// <inheritdoc />
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

            if (nextNumber > 999)
                nextNumber = 1;
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
