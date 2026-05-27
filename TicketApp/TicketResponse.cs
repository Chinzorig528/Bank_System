using System;
using System.Runtime.Serialization;

namespace BankTicket
{
    /// <summary>
    /// Bank API-аас буцаж ирдэг queue ticket object-ийг илэрхийлнэ.
    /// Data contract attribute-ууд нь API-ийн camelCase JSON field-үүдийг
    /// Windows Forms application-д ашиглаж байгаа C# property нэрүүдтэй тааруулж өгдөг.
    /// </summary>
    [DataContract]
    public class TicketResponse
    {
        /// <summary>
        /// Queue ticket-ийн database identifier.
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
        /// API-аас ирсэн createdAt талбарын түүхий string утга.
        /// ISO format болон timezone-той огноог DataContractJsonSerializer шууд DateTime болгож чаддаггүй
        /// тул эхлээд string болгон авч, дараа нь <see cref="CreatedAt"/> property дээр parse хийдэг.
        /// </summary>
        [DataMember(Name = "createdAt")]
        public string CreatedAtRaw { get; set; } = "";

        /// <summary>
        /// Ticket server дээр үүссэн огноо, цаг.
        /// API-аас ирсэн огноо уншигдахгүй format-тай бол <c>null</c> буцаана.
        /// </summary>
        public DateTime? CreatedAt
        {
            get
            {
                if (DateTimeOffset.TryParse(
                    CreatedAtRaw,
                    out DateTimeOffset createdAt))
                {
                    return createdAt.LocalDateTime;
                }

                return null;
            }
        }
    }
}
