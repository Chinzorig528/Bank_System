/// <summary>
/// Шинэ харилцагч бүртгэхэд API рүү ирэх өгөгдлийг дамжуулах DTO.
/// </summary>
public class RegisterCustomerDto
{
    /// <summary>
    /// Харилцагчийн бүтэн нэр.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Харилцагчийн утасны дугаар.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Харилцагчид холбох дансны дугаар.
    /// </summary>
    public string AccountNumber { get; set; }

    /// <summary>
    /// Харилцагчийн эхний үлдэгдэл.
    /// </summary>
    public decimal Balance { get; set; }
}
