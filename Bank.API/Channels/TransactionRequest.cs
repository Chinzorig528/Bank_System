public class TransactionRequest
{
    public string AccountNumber { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public TaskCompletionSource<bool> CompletionSource
        = new();
}