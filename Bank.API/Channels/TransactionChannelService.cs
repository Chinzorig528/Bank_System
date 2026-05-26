using System.Threading.Channels;

/// <summary>
/// AccountController болон TransactionWorker хоёрын хооронд гүйлгээний хүсэлт дамжуулах channel service.
/// Controller request бичиж, worker дарааллаар уншиж боловсруулна.
/// </summary>
public class TransactionChannelService
{
    /// <summary>
    /// Гүйлгээний request-үүдийг хадгалах unbounded channel.
    /// </summary>
    public Channel<TransactionRequest> Queue { get; }
        = Channel.CreateUnbounded<TransactionRequest>();
}
