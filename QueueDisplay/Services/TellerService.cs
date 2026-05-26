using BankDomain.Entities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QueueDisplay.Services;

/// <summary>
/// Teller app-аас Bank API руу queue дуудах хүсэлт илгээдэг service.
/// </summary>
public class TellerService
{
    private readonly HttpClient _client;

    /// <summary>
    /// API хүсэлт илгээх HttpClient-ийг онооно.
    /// </summary>
    /// <param name="client">Bank API-тэй холбогдох HttpClient.</param>
    public TellerService(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Дараагийн үйлчлүүлэгчийн queue дугаарыг API-аар дуудна.
    /// </summary>
    /// <returns>Дуудагдсан queue, байхгүй бол null.</returns>
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
