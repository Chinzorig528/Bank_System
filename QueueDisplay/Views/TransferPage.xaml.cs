using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace QueueDisplay.Views
{
    public sealed partial class TransferPage : Page
    {
        public TransferPage()
        {
            this.InitializeComponent();
        }

        private void Transfer_Click(
            object sender,
            RoutedEventArgs e)
        {
            string fromAccount =
                FromAccountBox.Text;

            string toAccount =
                ToAccountBox.Text;

            string amountText =
                AmountBox.Text;

            // validation

            if (string.IsNullOrWhiteSpace(fromAccount) ||
                string.IsNullOrWhiteSpace(toAccount) ||
                string.IsNullOrWhiteSpace(amountText))
            {
                ResultText.Foreground =
                    new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.Red);

                ResultText.Text =
                    "Please fill all fields.";

                return;
            }

            if (!decimal.TryParse(amountText, out decimal amount))
            {
                ResultText.Foreground =
                    new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.Red);

                ResultText.Text =
                    "Invalid amount.";

                return;
            }

            if (amount <= 0)
            {
                ResultText.Foreground =
                    new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.Red);

                ResultText.Text =
                    "Amount must be greater than 0.";

                return;
            }

            // success

            ResultText.Foreground =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Green);

            ResultText.Text =
                $"₮{amount:N0} transferred successfully\n" +
                $"From: {fromAccount}\n" +
                $"To: {toAccount}";
        }
    }
}