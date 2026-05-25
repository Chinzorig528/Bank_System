using BankDomain.Entities;

namespace BankApi.Channels;

public class QueueRequest
{
    public TaskCompletionSource<CustomerQueue?> Completion
        = new();
}