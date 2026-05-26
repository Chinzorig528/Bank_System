/// <summary>
/// Данснаас мөнгө авах request-ийн өгөгдлийг дамжуулах DTO.
/// </summary>
public class WithdrawDto
{
    /// <summary>
    /// Мөнгө авах дансны дугаар.
    /// </summary>
    public string AccountNumber { get; set; }

    /// <summary>
    /// Данснаас авах мөнгөн дүн.
    /// </summary>
    public decimal Amount { get; set; }
}
