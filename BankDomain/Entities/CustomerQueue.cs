namespace BankDomain.Entities;

public class CustomerQueue
{
    public int Id { get; set; }

    public string Number { get; set; } = "";

    public bool IsCalled { get; set; }

    public DateTime CreatedAt { get; set; }
}