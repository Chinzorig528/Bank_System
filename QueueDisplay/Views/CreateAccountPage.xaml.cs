using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Net.Http;
using System.Text;
using System;
using System.Text.Json;

namespace QueueDisplay.Views
{
    public sealed partial class CreateAccountPage : Page
    {
        private readonly HttpClient _http =
            new HttpClient();

        public CreateAccountPage()
        {
            this.InitializeComponent();

            _http.BaseAddress =
                new Uri("http://192.168.88.6:5092/");
        }

        // =========================
        // CREATE ACCOUNT
        // =========================

        private async void Create_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string accountNumber =
                    AccountNumberBox.Text;

                string balanceText =
                    BalanceBox.Text;

                // =========================
                // VALIDATION
                // =========================

                if (string.IsNullOrWhiteSpace(
                    accountNumber))
                {
                    ShowError(
                        "Enter account number");

                    return;
                }

                if (!decimal.TryParse(
                    balanceText,
                    out decimal balance))
                {
                    ShowError(
                        "Invalid balance");

                    return;
                }

                if (balance < 0)
                {
                    ShowError(
                        "Balance cannot be negative");

                    return;
                }

                // =========================
                // CREATE ACCOUNT
                // =========================

                var createResponse =
                    await _http.PostAsync(
                        $"account/create?accountNumber={accountNumber}",
                        null);

                if (!createResponse
                    .IsSuccessStatusCode)
                {
                    string error =
                        await createResponse.Content
                            .ReadAsStringAsync();

                    ShowError(error);

                    return;
                }

                // =========================
                // INITIAL DEPOSIT
                // =========================

                if (balance > 0)
                {
                    var dto = new
                    {
                        AccountNumber =
                            accountNumber,

                        Amount = balance
                    };

                    var json =
                        JsonSerializer.Serialize(dto);

                    var content =
                        new StringContent(
                            json,
                            Encoding.UTF8,
                            "application/json");

                    var depositResponse =
                        await _http.PostAsync(
                            "account/deposit",
                            content);

                    if (!depositResponse
                        .IsSuccessStatusCode)
                    {
                        ShowError(
                            "Deposit failed");

                        return;
                    }
                }

                // =========================
                // SUCCESS
                // =========================

                ResultText.Foreground =
                    new SolidColorBrush(
                        Colors.Green);

                ResultText.Text =
                    $"Account created successfully\n" +
                    $"Account: {accountNumber}\n" +
                    $"Balance: ₮{balance:N0}";
            }
            catch (Exception ex)
            {
                ShowError(
                    ex.Message);
            }
        }

        // =========================
        // ERROR
        // =========================

        private void ShowError(
            string message)
        {
            ResultText.Foreground =
                new SolidColorBrush(
                    Colors.Red);

            ResultText.Text =
                message;
        }
    }
}