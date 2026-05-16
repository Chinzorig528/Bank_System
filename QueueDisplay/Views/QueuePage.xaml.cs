using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QueueDisplay.Services;
using System;
using System.Net.Http;

namespace QueueDisplay.Views
{
    public sealed partial class QueuePage : Page
    {
        private readonly TellerService _service;

        public QueuePage()
        {
            this.InitializeComponent();

            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5122/")
            };

            _service = new TellerService(client);
        }

        private async void CallNext_Click(
            object sender,
            RoutedEventArgs e)
        {
            var ticket = await _service.CallNextAsync();

            if (ticket == null)
            {
                TicketText.Text = "Queue Empty";
                return;
            }

            TicketText.Text = ticket.Number;
        }
    }
}