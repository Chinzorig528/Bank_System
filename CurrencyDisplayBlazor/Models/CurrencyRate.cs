namespace CurrencyDisplayBlazor.Models
{
    /// <summary>
    /// Blazor ханшийн дэлгэц дээр харуулах валютын ханшийн model.
    /// </summary>
    public class CurrencyRate
    {
        /// <summary>
        /// Валютын ханшийн ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Валютын богино код.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Валютын нэр.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Банк худалдаж авах ханш.
        /// </summary>
        public decimal BuyRate { get; set; }

        /// <summary>
        /// Банк зарах ханш.
        /// </summary>
        public decimal SellRate { get; set; }

        /// <summary>
        /// Ханш сүүлд шинэчлэгдсэн огноо.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
