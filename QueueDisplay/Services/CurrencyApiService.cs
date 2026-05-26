using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TellerApp.Models;

namespace TellerApp.Services
{
    public class CurrencyApiService
    {
        private readonly HttpClient _httpClient;

        public CurrencyApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://192.168.88.6:5092/");
            };
        }

        public async Task<List<CurrencyRate>> GetCurrenciesAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<List<CurrencyRate>>("api/Currency");

            return result ?? new List<CurrencyRate>();
        }

        public async Task UpdateCurrencyAsync(CurrencyRate currency)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/Currency/{currency.Id}",
                currency
            );

            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAllCurrenciesAsync(List<CurrencyRate> currencies)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/Currency/update-all",
                currencies
            );

            response.EnsureSuccessStatusCode();
        }
    }
}