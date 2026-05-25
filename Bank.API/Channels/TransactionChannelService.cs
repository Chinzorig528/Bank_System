using System.Threading.Channels;

public class TransactionChannelService
{
    public Channel<TransactionRequest> Queue { get; }
        = Channel.CreateUnbounded<TransactionRequest>();
}