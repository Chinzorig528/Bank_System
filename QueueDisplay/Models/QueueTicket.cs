namespace TellerApp.Models;

public class QueueTicket
{
    public int Id { get; set; }

    public string TicketNumber { get; set; }

    public string ServiceType { get; set; }

    public string Status { get; set; }
}