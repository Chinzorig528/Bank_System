namespace Bank.Domain.Entities
{
    /// <summary>
    /// Банкны харилцагчийн үндсэн мэдээллийг илэрхийлнэ.
    /// Харилцагчийн нэр, утас, дансны дугаар, үлдэгдэл зэрэг мэдээллийг хадгална.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Database дээрх customer record-ийн primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Харилцагчийн бүтэн нэр.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Харилцагчийн холбоо барих утасны дугаар.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Харилцагчтай холбоотой банкны дансны дугаар.
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Харилцагчийн дансны одоогийн үлдэгдэл.
        /// </summary>
        public decimal Balance { get; set; }
    }
}
