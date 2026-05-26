namespace Bank.Application.DTOs
{
    /// <summary>
    /// Нэг данснаас нөгөө данс руу шилжүүлэг хийх request-ийн өгөгдлийг дамжуулах DTO.
    /// </summary>
    public class TransferDto
    {
        /// <summary>
        /// Мөнгө гарах дансны дугаар.
        /// </summary>
        public string FromAccount { get; set; }

        /// <summary>
        /// Мөнгө хүлээн авах дансны дугаар.
        /// </summary>
        public string ToAccount { get; set; }

        /// <summary>
        /// Шилжүүлэх мөнгөн дүн.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
