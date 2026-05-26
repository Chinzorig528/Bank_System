namespace Bank.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string AccountNumber { get; set; }

        public decimal Balance { get; set; }
    }
}