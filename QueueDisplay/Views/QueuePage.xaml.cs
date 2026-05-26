using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QueueDisplay.Services;
using System;
using System.Net.Http;
using TellerApp.Services;

namespace QueueDisplay.Views
{
    public sealed partial class QueuePage : Page
    {
        private readonly TellerService _service;
        private readonly string _tellerId =
            QueueAppSettings.TellerId;

        public QueuePage()
        {
            this.InitializeComponent();

            var client = new HttpClient
            {
                BaseAddress = new Uri(QueueAppSettings.ApiBaseUrl)
            };

            _service = new TellerService(client);

            TellerText.Text =
                _tellerId;
        }

        private async void CallNext_Click(
            object sender,
            RoutedEventArgs e)
        {
            var ticket =
                await _service.CallNextAsync();

            if (ticket == null)
            {
                TicketText.Text =
                    "Queue Empty";

                return;
            }

            TicketText.Text =
                ticket.Number;

            SocketSenderService socket =
                new SocketSenderService();

            await socket.SendQueueAsync(
                _tellerId,
                ticket.Number,
                QueueAppSettings.SocketHost,
                QueueAppSettings.SocketPort);
        }
    }
}
