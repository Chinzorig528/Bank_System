using Newtonsoft.Json;
using System.Threading.Tasks;
using TellerApp.Models;
using System.Net.Http;
using System;
using QueueDisplay.Services;

namespace TellerApp.Services;

/// <summary>
/// Teller app-ийн хуучин ticket API дуудлагуудыг гүйцэтгэх service.
/// </summary>
public class ApiService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Ticket API service үүсгэж API серверийн үндсэн хаягийг тохируулна.
    /// </summary>
    public ApiService()
    {
        _httpClient = new HttpClient();

        _httpClient.BaseAddress =
            new Uri(QueueAppSettings.ApiBaseUrl);
    }

    /// <summary>
    /// API-аас дараагийн ticket дугаарыг авна.
    /// </summary>
    /// <returns>Дараагийн ticket, байхгүй эсвэл алдаа гарвал null.</returns>
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

    /// <summary>
    /// Ticket-ийг дууссан төлөвт шилжүүлэх хүсэлт илгээнэ.
    /// </summary>
    /// <param name="id">Дуусгах ticket-ийн ID.</param>
    public async Task CompleteTicket(int id)
    {
        await _httpClient.PostAsync(
            $"api/queue/complete/{id}",
            null);
    }
}
