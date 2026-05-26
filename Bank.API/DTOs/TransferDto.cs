namespace Bank.Application.DTOs
{
    public class TransferDto
    {
        public string FromAccount { get; set; }

        public string ToAccount { get; set; }

        public decimal Amount { get; set; }
    }
}