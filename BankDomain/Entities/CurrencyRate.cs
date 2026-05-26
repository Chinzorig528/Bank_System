namespace BankDomain.Entities;

/// <summary>
/// Валютын ханшийн мэдээллийг хадгалах entity.
/// Нэг record нь валютын код, нэр, авах ханш, зарах ханш, сүүлд шинэчлэгдсэн хугацааг агуулна.
/// </summary>
public class CurrencyRate
{
    /// <summary>
    /// Database дээрх currency rate record-ийн primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Валютын ISO код. Жишээ нь <c>USD</c>, <c>EUR</c>.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Валютын дэлгэрэнгүй нэр.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Банк тухайн валютыг авах ханш.
    /// </summary>
    public decimal BuyRate { get; set; }

    /// <summary>
    /// Банк тухайн валютыг зарах ханш.
    /// </summary>
    public decimal SellRate { get; set; }

    /// <summary>
    /// Ханш хамгийн сүүлд шинэчлэгдсэн огноо, цаг.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
