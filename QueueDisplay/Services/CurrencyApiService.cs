using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using QueueDisplay.Services;
using TellerApp.Models;

namespace TellerApp.Services
{
    /// <summary>
    /// Teller app-аас валютын ханшийн API-тай ажиллах service.
    /// </summary>
    public class CurrencyApiService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Валютын API service үүсгэж үндсэн API URL-г тохируулна.
        /// </summary>
        public CurrencyApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(QueueAppSettings.ApiBaseUrl)
            };
        }

        /// <summary>
        /// API-аас бүх валютын ханшийг авна.
        /// </summary>
        /// <returns>Валютын ханшийн жагсаалт.</returns>
        public async Task<List<CurrencyRate>> GetCurrenciesAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<List<CurrencyRate>>("api/Currency");

            return result ?? new List<CurrencyRate>();
        }

        /// <summary>
        /// Нэг валютын ханшийг API дээр шинэчилнэ.
        /// </summary>
        /// <param name="currency">Шинэчлэх валютын ханш.</param>
        public async Task UpdateCurrencyAsync(CurrencyRate currency)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/Currency/{currency.Id}",
                currency
            );

            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Олон валютын ханшийг нэг дор API дээр шинэчилнэ.
        /// </summary>
        /// <param name="currencies">Шинэчлэх валютын ханшууд.</param>
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
