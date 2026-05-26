using System.Threading.Channels;

namespace BankApi.Channels;

/// <summary>
/// QueueController болон QueueWorker хоёрын хооронд queue call request дамжуулах channel service.
/// Ингэснээр олон teller зэрэг дуудах үед хүсэлтүүд дарааллаар боловсруулагдана.
/// </summary>
public class QueueChannelService
{
    /// <summary>
    /// Queue call request-үүдийг хадгалах unbounded channel.
    /// </summary>
    public Channel<QueueRequest> Queue
        = Channel.CreateUnbounded<QueueRequest>();
}
