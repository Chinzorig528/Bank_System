/// <summary>
/// Дансанд мөнгө хийх request-ийн өгөгдлийг дамжуулах DTO.
/// </summary>
public class DepositDto
{
    /// <summary>
    /// Мөнгө хийх дансны дугаар.
    /// </summary>
    public string AccountNumber { get; set; }

    /// <summary>
    /// Дансанд нэмэх мөнгөн дүн.
    /// </summary>
    public decimal Amount { get; set; }
}
