using System;

namespace TellerApp.Models
{
    /// <summary>
    /// Teller app дээр харуулах валютын ханшийн мэдээлэл.
    /// </summary>
    public class CurrencyRate
    {
        /// <summary>
        /// Валютын ханшийн ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Валютын богино код. Жишээ нь USD, EUR.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Валютын Монгол нэр.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Банк валют худалдаж авах ханш.
        /// </summary>
        public decimal BuyRate { get; set; }

        /// <summary>
        /// Банк валют зарах ханш.
        /// </summary>
        public decimal SellRate { get; set; }

        /// <summary>
        /// Ханш хамгийн сүүлд шинэчлэгдсэн огноо.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
