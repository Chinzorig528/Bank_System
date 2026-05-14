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
                await _client.PostAsync("queue", null);

            if (!response.IsSuccessStatusCode)
                return null;

            var ticket =
                await response.Content
                .ReadFromJsonAsync<TicketResponse>();

            if (ticket == null)
                return null;

            if (string.IsNullOrWhiteSpace(ticket.Number))
                return null;

            return ticket;
        }
    }
}