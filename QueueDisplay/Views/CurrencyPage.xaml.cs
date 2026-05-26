using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QueueDisplay.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using TellerApp.Models;
using TellerApp.Services;

namespace TellerApp.Views
{
    public sealed partial class CurrencyPage : Page
    {
        private readonly CurrencyApiService _currencyApiService;
        private HubConnection? _hubConnection;

        public ObservableCollection<CurrencyRateEdit> Currencies { get; set; }

        public CurrencyPage()
        {
            InitializeComponent();

            _currencyApiService = new CurrencyApiService();
            Currencies = new ObservableCollection<CurrencyRateEdit>();

            Loaded += CurrencyPage_Loaded;
            Unloaded += CurrencyPage_Unloaded;
        }

        private async void CurrencyPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCurrenciesAsync();
            await ConnectCurrencyHubAsync();
        }

        private async Task LoadCurrenciesAsync()
        {
            try
            {
                StatusText.Text = "Loading currency rates...";

                List<CurrencyRate> currencies =
                    await _currencyApiService.GetCurrenciesAsync();

                ApplyCurrencies(currencies);

                StatusText.Text = "Currency rates loaded.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Failed to load currency rates: " + ex.Message;
            }
        }

        private async Task ConnectCurrencyHubAsync()
        {
            if (_hubConnection != null)
                return;

            string hubUrl =
                new Uri(new Uri(QueueAppSettings.ApiBaseUrl), "currencyHub").ToString();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<List<CurrencyRate>>(
                "ReceiveCurrencyRates",
                updatedRates =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ApplyCurrencies(updatedRates);
                        StatusText.Text = "Realtime currency rates updated.";
                    });
                });

            _hubConnection.Reconnecting += error =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    StatusText.Text = "Realtime reconnecting...";
                });

                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += connectionId =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    StatusText.Text = "Realtime reconnected.";
                });

                return Task.CompletedTask;
            };

            _hubConnection.Closed += async error =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    StatusText.Text = "Realtime disconnected. Reconnecting...";
                });

                await Task.Delay(2000);

                if (_hubConnection != null)
                    await _hubConnection.StartAsync();
            };

            await _hubConnection.StartAsync();

            StatusText.Text = "Realtime connected.";
        }

        private void ApplyCurrencies(List<CurrencyRate> currencies)
        {
            Currencies.Clear();

            foreach (CurrencyRate currency in currencies)
            {
                Currencies.Add(new CurrencyRateEdit
                {
                    Id = currency.Id,
                    Code = currency.Code,
                    Name = currency.Name,
                    BuyRate = currency.BuyRate.ToString(CultureInfo.InvariantCulture),
                    SellRate = currency.SellRate.ToString(CultureInfo.InvariantCulture),
                    UpdatedAt = currency.UpdatedAt
                });
            }

            CurrencyListView.ItemsSource = Currencies;
        }

        private CurrencyRate ConvertToCurrencyRate(CurrencyRateEdit edit)
        {
            decimal buyRate = decimal.Parse(edit.BuyRate, CultureInfo.InvariantCulture);
            decimal sellRate = decimal.Parse(edit.SellRate, CultureInfo.InvariantCulture);

            return new CurrencyRate
            {
                Id = edit.Id,
                Code = edit.Code,
                Name = edit.Name,
                BuyRate = buyRate,
                SellRate = sellRate,
                UpdatedAt = DateTime.Now
            };
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadCurrenciesAsync();
        }

        private async void UpdateSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrencyListView.SelectedItem is not CurrencyRateEdit selectedCurrency)
                {
                    StatusText.Text = "Select a currency row first.";
                    return;
                }

                CurrencyRate currency = ConvertToCurrencyRate(selectedCurrency);

                await _currencyApiService.UpdateCurrencyAsync(currency);

                StatusText.Text =
                    $"{currency.Code} updated. Buy: {currency.BuyRate}, Sell: {currency.SellRate}";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Failed to update selected currency: " + ex.Message;
            }
        }

        private async void UpdateAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<CurrencyRate> updatedCurrencies =
                    new List<CurrencyRate>();

                foreach (CurrencyRateEdit editCurrency in Currencies)
                {
                    CurrencyRate currency =
                        ConvertToCurrencyRate(editCurrency);

                    updatedCurrencies.Add(currency);
                }

                await _currencyApiService.UpdateAllCurrenciesAsync(updatedCurrencies);

                StatusText.Text = "All currency rates updated.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Failed to update all currency rates: " + ex.Message;
            }
        }

        private async void CurrencyPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_hubConnection == null)
                return;

            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }
}
