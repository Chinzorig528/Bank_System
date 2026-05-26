using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace BankTicket
{
    /// <summary>
    /// Ticket desktop application болон Bank API-ийн queue endpoint хоорондын харилцааг хариуцна.
    /// Энэ service нь зөвхөн шинэ queue ticket үүсгэх хүсэлт илгээж, API-аас ирсэн JSON хариуг
    /// Windows Forms UI дээр харуулах, хэвлэх боломжтой <see cref="TicketResponse"/> object болгон хөрвүүлнэ.
    /// </summary>
    public class TicketService
    {
        private readonly HttpClient _client;

        /// <summary>
        /// API request явуулахад ашиглах <see cref="HttpClient"/>-тэй ticket service үүсгэнэ.
        /// Энэ service-ийг ашиглахаас өмнө дуудсан тал нь <see cref="HttpClient.BaseAddress"/>-ийг тохируулах ёстой,
        /// учир нь <see cref="CreateTicketAsync"/> method нь <c>api/queue</c> гэсэн relative endpoint рүү хүсэлт илгээдэг.
        /// </summary>
        /// <param name="client">
        /// Queue API рүү ticket үүсгэх хүсэлт илгээхэд ашиглах HTTP client.
        /// </param>
        public TicketService(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// <c>api/queue</c> endpoint рүү POST request илгээж API-аас шинэ ticket дугаар авна.
        /// API амжилттай status code буцаавал JSON хариуг <see cref="TicketResponse"/> object болгон уншина.
        /// Харин API алдаатай status code буцаавал <c>null</c> буцааж, UI хэрэглэгчид ойлгомжтой алдааны мэдэгдэл
        /// харуулах боломжтой болгоно.
        /// </summary>
        /// <returns>
        /// Үүссэн ticket-ийн мэдээлэлтэй <see cref="TicketResponse"/>, эсвэл API амжилтгүй status code буцаасан үед <c>null</c>.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// API server-т холбогдож чадахгүй, эсвэл response ирэхээс өмнө HTTP request амжилтгүй болсон үед гарна.
        /// </exception>
        /// <exception cref="TaskCanceledException">
        /// Request timeout болох эсвэл цуцлагдах үед гарна.
        /// </exception>
        public async Task<TicketResponse> CreateTicketAsync()
        {
            HttpResponseMessage response =
                await _client.PostAsync(
                    "api/queue",
                    null);

            if (!response.IsSuccessStatusCode)
                return null;

            string json =
                await response.Content.ReadAsStringAsync();

            using (var stream =
                new MemoryStream(
                    Encoding.UTF8.GetBytes(json)))
            {
                var serializer =
                    new DataContractJsonSerializer(
                        typeof(TicketResponse));

                return serializer.ReadObject(stream)
                    as TicketResponse;
            }
        }
    }
}
