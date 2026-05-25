using System.Threading.Channels;

namespace BankApi.Channels;

public class QueueChannelService
{
    public Channel<QueueRequest> Queue
        = Channel.CreateUnbounded<QueueRequest>();
}