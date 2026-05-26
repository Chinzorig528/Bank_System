namespace TellerApp.Models;

/// <summary>
/// Teller app дээр харагдах ticket буюу queue дугаарын мэдээлэл.
/// </summary>
public class QueueTicket
{
    /// <summary>
    /// Ticket-ийн дотоод ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Дэлгэц дээр харуулах ticket дугаар.
    /// </summary>
    public string TicketNumber { get; set; }

    /// <summary>
    /// Ticket ямар үйлчилгээний төрөлд хамаарахыг илэрхийлнэ.
    /// </summary>
    public string ServiceType { get; set; }

    /// <summary>
    /// Ticket-ийн одоогийн төлөв.
    /// </summary>
    public string Status { get; set; }
}
