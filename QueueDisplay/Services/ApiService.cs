using Newtonsoft.Json;
using System.Threading.Tasks;
using TellerApp.Models;
using System.Net.Http;
using System;

namespace TellerApp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient();

        _httpClient.BaseAddress =
            new Uri("https://localhost:7001/");
    }

    public async Task<QueueTicket?> GetNextTicket()
    {
        var response =
            await _httpClient.GetAsync("api/queue/next");

        if (!response.IsSuccessStatusCode)
            return null;

        var json =
            await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<QueueTicket>(json);
    }

    public async Task CompleteTicket(int id)
    {
        await _httpClient.PostAsync(
            $"api/queue/complete/{id}",
            null);
    }
}