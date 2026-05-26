namespace BankDomain.Entities;

/// <summary>
/// Харилцагчийн дарааллын нэг ticket record-ийг илэрхийлнэ.
/// Энэ entity нь ticket дугаар, дуудагдсан эсэх төлөв, үүссэн огноог хадгална.
/// </summary>
public class CustomerQueue
{
    /// <summary>
    /// Database дээрх queue record-ийн primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Хэрэглэгчид харагдах дарааллын дугаар. Жишээ нь <c>A001</c>.
    /// </summary>
    public string Number { get; set; } = "";

    /// <summary>
    /// Энэ ticket-ийг teller дуудаж дууссан эсэх төлөв.
    /// </summary>
    public bool IsCalled { get; set; }

    /// <summary>
    /// Ticket үүссэн огноо, цаг.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
