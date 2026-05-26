using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace QueueDisplay.Views
{
    public sealed partial class TransferPage : Page
    {
        private readonly HttpClient _http =
            new HttpClient();

        public TransferPage()
        {
            this.InitializeComponent();

            // API ADDRESS

            _http.BaseAddress =
                new Uri("https://192.168.88.6:5092/");
        }

        // =====================================================
        // LOAD BALANCE
        // =====================================================

        private async void LoadBalance_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string account =
                    BalanceAccountBox.Text;

                if (string.IsNullOrWhiteSpace(account))
                {
                    ShowBalanceError(
                        "Enter account number");

                    return;
                }

                var response =
                    await _http.GetAsync(
                        $"account/balance/{account}");

                if (!response.IsSuccessStatusCode)
                {
                    ShowBalanceError(
                        "Account not found");

                    return;
                }

                string result =
                    await response.Content
                        .ReadAsStringAsync();

                BalanceText.Foreground =
                    new SolidColorBrush(
                        Colors.Blue);

                BalanceText.Text =
                    $"Balance: ₮{result}";
            }
            catch (Exception ex)
            {
                ShowBalanceError(
                    ex.Message);
            }
        }

        // =====================================================
        // DEPOSIT
        // =====================================================

        private async void Deposit_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string account =
                    DepositAccountBox.Text;

                string amountText =
                    DepositAmountBox.Text;

                // VALIDATION

                if (string.IsNullOrWhiteSpace(account) ||
                    string.IsNullOrWhiteSpace(amountText))
                {
                    ShowDepositError(
                        "Fill all fields");

                    return;
                }

                if (!decimal.TryParse(
                    amountText,
                    out decimal amount))
                {
                    ShowDepositError(
                        "Invalid amount");

                    return;
                }

                if (amount <= 0)
                {
                    ShowDepositError(
                        "Amount must be greater than 0");

                    return;
                }

                // DTO

                var dto = new
                {
                    AccountNumber = account,
                    Amount = amount
                };

                var json =
                    JsonSerializer.Serialize(dto);

                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                // API CALL

                var response =
                    await _http.PostAsync(
                        "account/deposit",
                        content);

                if (!response.IsSuccessStatusCode)
                {
                    string error =
                        await response.Content
                            .ReadAsStringAsync();

                    ShowDepositError(
                        error);

                    return;
                }

                DepositResultText.Foreground =
                    new SolidColorBrush(
                        Colors.Green);

                DepositResultText.Text =
                    $"₮{amount:N0} deposited successfully";
            }
            catch (Exception ex)
            {
                ShowDepositError(
                    ex.Message);
            }
        }

        // =====================================================
        // WITHDRAW
        // =====================================================

        private async void Withdraw_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string account =
                    WithdrawAccountBox.Text;

                string amountText =
                    WithdrawAmountBox.Text;

                // VALIDATION

                if (string.IsNullOrWhiteSpace(account) ||
                    string.IsNullOrWhiteSpace(amountText))
                {
                    ShowWithdrawError(
                        "Fill all fields");

                    return;
                }

                if (!decimal.TryParse(
                    amountText,
                    out decimal amount))
                {
                    ShowWithdrawError(
                        "Invalid amount");

                    return;
                }

                if (amount <= 0)
                {
                    ShowWithdrawError(
                        "Amount must be greater than 0");

                    return;
                }

                // DTO

                var dto = new
                {
                    AccountNumber = account,
                    Amount = amount
                };

                var json =
                    JsonSerializer.Serialize(dto);

                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                // API CALL

                var response =
                    await _http.PostAsync(
                        "account/withdraw",
                        content);

                if (!response.IsSuccessStatusCode)
                {
                    string error =
                        await response.Content
                            .ReadAsStringAsync();

                    ShowWithdrawError(
                        error);

                    return;
                }

                WithdrawResultText.Foreground =
                    new SolidColorBrush(
                        Colors.Green);

                WithdrawResultText.Text =
                    $"₮{amount:N0} withdrawn successfully";
            }
            catch (Exception ex)
            {
                ShowWithdrawError(
                    ex.Message);
            }
        }

        // =====================================================
        // TRANSFER
        // =====================================================

        private async void Transfer_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string from =
                    FromAccountBox.Text;

                string to =
                    ToAccountBox.Text;

                string amountText =
                    TransferAmountBox.Text;

                // VALIDATION

                if (string.IsNullOrWhiteSpace(from) ||
                    string.IsNullOrWhiteSpace(to) ||
                    string.IsNullOrWhiteSpace(amountText))
                {
                    ShowTransferError(
                        "Fill all fields");

                    return;
                }

                if (!decimal.TryParse(
                    amountText,
                    out decimal amount))
                {
                    ShowTransferError(
                        "Invalid amount");

                    return;
                }

                if (amount <= 0)
                {
                    ShowTransferError(
                        "Amount must be greater than 0");

                    return;
                }

                if (from == to)
                {
                    ShowTransferError(
                        "Cannot transfer to same account");

                    return;
                }

                // DTO

                var dto = new
                {
                    FromAccount = from,
                    ToAccount = to,
                    Amount = amount
                };

                var json =
                    JsonSerializer.Serialize(dto);

                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                // API CALL

                var response =
                    await _http.PostAsync(
                        "account/transfer",
                        content);

                if (!response.IsSuccessStatusCode)
                {
                    string error =
                        await response.Content
                            .ReadAsStringAsync();

                    ShowTransferError(
                        error);

                    return;
                }

                TransferResultText.Foreground =
                    new SolidColorBrush(
                        Colors.Green);

                TransferResultText.Text =
                    $"₮{amount:N0} transferred successfully";
            }
            catch (Exception ex)
            {
                ShowTransferError(
                    ex.Message);
            }
        }

        // =====================================================
        // ERROR METHODS
        // =====================================================

        private void ShowBalanceError(
            string message)
        {
            BalanceText.Foreground =
                new SolidColorBrush(
                    Colors.Red);

            BalanceText.Text =
                message;
        }

        private void ShowDepositError(
            string message)
        {
            DepositResultText.Foreground =
                new SolidColorBrush(
                    Colors.Red);

            DepositResultText.Text =
                message;
        }

        private void ShowWithdrawError(
            string message)
        {
            WithdrawResultText.Foreground =
                new SolidColorBrush(
                    Colors.Red);

            WithdrawResultText.Text =
                message;
        }

        private void ShowTransferError(
            string message)
        {
            TransferResultText.Foreground =
                new SolidColorBrush(
                    Colors.Red);

            TransferResultText.Text =
                message;
        }
    }
}