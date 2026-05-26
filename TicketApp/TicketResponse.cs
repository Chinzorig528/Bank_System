using System;
using System.Runtime.Serialization;

namespace BankTicket
{
    /// <summary>
    /// Bank API-аас буцаж ирдэг queue ticket object-ийг илэрхийлнэ.
    /// Data contract attribute-ууд нь API-ийн camelCase JSON field-үүдийг Windows Forms application-д ашиглаж байгаа
    /// C# property нэрүүдтэй тааруулж өгдөг.
    /// </summary>
    [DataContract]
    public class TicketResponse
    {
        /// <summary>
        /// Queue ticket-ийн database identifier.
        /// Энэ утга нь голчлон API талд хадгалагдсан record-ийг мөрдөхөд хэрэгтэй.
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Хэрэглэгчид харагдах ticket дугаар. Жишээ нь <c>A001</c>.
        /// </summary>
        [DataMember(Name = "number")]
        public string Number { get; set; } = "";

        /// <summary>
        /// Энэ ticket-ийг teller аль хэдийн дуудсан эсэхийг илэрхийлнэ.
        /// Шинээр үүссэн ticket ердийн үед <c>false</c> байна.
        /// </summary>
        [DataMember(Name = "isCalled")]
        public bool IsCalled { get; set; }

        /// <summary>
        /// Ticket server дээр үүссэн огноо, цаг.
        /// </summary>
        [DataMember(Name = "createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
