using BankDomain.Entities;

namespace BankApi.Channels;

/// <summary>
/// QueueController-оос QueueWorker рүү дараагийн ticket дуудах хүсэлт дамжуулах object.
/// Worker боловсруулсны дараа олдсон ticket эсвэл <c>null</c> утгыг <see cref="Completion"/> дээр тавина.
/// </summary>
public class QueueRequest
{
    /// <summary>
    /// Дараагийн queue ticket-ийн үр дүнг controller рүү буцаах completion source.
    /// </summary>
    public TaskCompletionSource<CustomerQueue?> Completion
        = new();
}
