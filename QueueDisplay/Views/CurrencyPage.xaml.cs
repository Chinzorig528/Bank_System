using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

        public ObservableCollection<CurrencyRateEdit> Currencies { get; set; }

        public CurrencyPage()
        {
            InitializeComponent();

            _currencyApiService = new CurrencyApiService();
            Currencies = new ObservableCollection<CurrencyRateEdit>();

            Loaded += CurrencyPage_Loaded;
        }

        private async void CurrencyPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCurrenciesAsync();
        }

        private async Task LoadCurrenciesAsync()
        {
            try
            {
                StatusText.Text = "Ханш татаж байна...";

                Currencies.Clear();

                List<CurrencyRate> currencies =
                    await _currencyApiService.GetCurrenciesAsync();

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

                StatusText.Text = "Ханш амжилттай татагдлаа.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ханш татах үед алдаа гарлаа: " + ex.Message;
            }
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
                    StatusText.Text = "Эхлээд шинэчлэх валютын мөрөө сонгоно уу.";
                    return;
                }

                CurrencyRate currency = ConvertToCurrencyRate(selectedCurrency);

                await _currencyApiService.UpdateCurrencyAsync(currency);

                StatusText.Text =
                    $"{currency.Code} шинэчлэгдлээ. Авах: {currency.BuyRate}, Зарах: {currency.SellRate}";

                await LoadCurrenciesAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text = "Сонгосон ханш шинэчлэх үед алдаа гарлаа: " + ex.Message;
            }
        }

        private async void UpdateAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<CurrencyRate> updatedCurrencies = new List<CurrencyRate>();

                foreach (CurrencyRateEdit editCurrency in Currencies)
                {
                    CurrencyRate currency = ConvertToCurrencyRate(editCurrency);
                    updatedCurrencies.Add(currency);
                }

                await _currencyApiService.UpdateAllCurrenciesAsync(updatedCurrencies);

                StatusText.Text = "Бүх валютын ханш шинэчлэгдлээ.";

                await LoadCurrenciesAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text = "Бүх ханш шинэчлэх үед алдаа гарлаа: " + ex.Message;
            }
        }
    }
}