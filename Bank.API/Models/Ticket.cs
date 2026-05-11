namespace BankApi.Models;

public class Ticket
{
    public int Id { get; set; }

    public string Number { get; set; } = "";

    public string ServiceType { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}