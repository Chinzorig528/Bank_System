using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BankTicket
{

    public class TicketService
    {
        private readonly HttpClient _client;

        public TicketService(HttpClient client)
        {
            _client = client;
        }

        public async Task<TicketResponse> CreateTicketAsync()
        {
            HttpResponseMessage response =
                await _client.PostAsync(
                    "api/queue",
                    null);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<TicketResponse>();
        }
    }
}