using BankDomain.Entities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QueueDisplay.Services;

public class TellerService
{
    private readonly HttpClient _client;

    public TellerService(HttpClient client)
    {
        _client = client;
    }

    public async Task<CustomerQueue?> CallNextAsync()
    {
        var response =
            await _client.PostAsync(
                "api/queue/next",
                null);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content
            .ReadFromJsonAsync<CustomerQueue>();
    }
}