namespace BankDomain.Entities
{
    /// <summary>
    /// Банкны дансны мэдээллийг илэрхийлнэ.
    /// Дансны дугаар, үлдэгдэл, үүссэн огноог хадгална.
    /// </summary>
    public class BankAccount
    {
        /// <summary>
        /// Database дээрх account record-ийн primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Банкны дансны давтагдашгүй дугаар.
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Дансны одоогийн үлдэгдэл.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Данс үүссэн огноо, цаг.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
