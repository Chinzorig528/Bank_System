using BankDomain.Entities;

namespace BankInfrastructure.Interfaces;

public interface IQueueRepository
{
    Task AddAsync(CustomerQueue queue);

    Task<List<CustomerQueue>> GetAllAsync();

    Task<CustomerQueue?> GetNextAsync();

    Task SaveChangesAsync();

    Task<CustomerQueue?> GetLastQueueAsync();
}