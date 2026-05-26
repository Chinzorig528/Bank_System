/// <summary>
/// AccountController-оос TransactionWorker рүү дамжуулах гүйлгээний хүсэлт.
/// Channel-ээр дамжсан request бүр боловсруулагдаад <see cref="CompletionSource"/> дээр үр дүнгээ тавина.
/// </summary>
public class TransactionRequest
{
    /// <summary>
    /// Гүйлгээ хийх дансны дугаар.
    /// </summary>
    public string AccountNumber { get; set; }

    /// <summary>
    /// Гүйлгээний мөнгөн дүн.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Гүйлгээний төрөл.
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Worker гүйлгээг амжилттай боловсруулсан эсэхийг controller рүү буцаах completion source.
    /// </summary>
    public TaskCompletionSource<bool> CompletionSource
        = new();
}
