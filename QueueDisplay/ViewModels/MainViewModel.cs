using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using TellerApp.Models;
using TellerApp.Services;

namespace TellerApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private QueueTicket currentTicket;

    public MainViewModel()
    {
        _apiService = new ApiService();
    }

    [RelayCommand]
    public async Task CallNext()
    {
        CurrentTicket =
            await _apiService.GetNextTicket();
    }

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