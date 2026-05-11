using BankDomain.Entities;

namespace BankServices.Interfaces;

public interface IQueueService
{
    Task<CustomerQueue> CreateQueueAsync();

    Task<CustomerQueue?> CallNextAsync();

    Task<List<CustomerQueue>> GetAllAsync();
}