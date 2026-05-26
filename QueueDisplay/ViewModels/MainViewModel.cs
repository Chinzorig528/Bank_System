using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using TellerApp.Models;
using TellerApp.Services;

namespace TellerApp.ViewModels;

/// <summary>
/// Teller app-ийн үндсэн дэлгэц дээрх ticket дуудах болон дуусгах үйлдлийн view model.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    /// <summary>
    /// Одоогоор дэлгэц дээр байгаа ticket.
    /// </summary>
    [ObservableProperty]
    private QueueTicket currentTicket;

    /// <summary>
    /// View model үүсгэж API service-ийг бэлдэнэ.
    /// </summary>
    public MainViewModel()
    {
        _apiService = new ApiService();
    }

    /// <summary>
    /// Дараагийн ticket-ийг API-аас авч дэлгэц дээр онооно.
    /// </summary>
    [RelayCommand]
    public async Task CallNext()
    {
        CurrentTicket =
            await _apiService.GetNextTicket();
    }

    /// <summary>
    /// Одоогийн ticket-ийг дуусгаж дэлгэцээс цэвэрлэнэ.
    /// </summary>
    [RelayCommand]
    public async Task Complete()
    {
        if (CurrentTicket == null)
            return;

        await _apiService
            .CompleteTicket(CurrentTicket.Id);

        CurrentTicket = null;
    }
}
