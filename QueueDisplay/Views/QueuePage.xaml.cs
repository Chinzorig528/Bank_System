using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QueueDisplay.Services;
using System;
using System.Net.Http;
using TellerApp.Services;

namespace QueueDisplay.Views
{
    /// <summary>
    /// Teller-ийн queue дуудах хуудас.
    /// </summary>
    public sealed partial class QueuePage : Page
    {
        private readonly TellerService _service;
        private readonly string _tellerId =
            QueueAppSettings.TellerId;

        /// <summary>
        /// Queue page үүсгэж API service болон teller ID-г дэлгэц дээр бэлдэнэ.
        /// </summary>
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

        /// <summary>
        /// Next товч дарахад дараагийн queue дугаарыг API-аас дуудаж socket display руу илгээнэ.
        /// </summary>
        /// <param name="sender">Event үүсгэсэн control.</param>
        /// <param name="e">Click event-ийн мэдээлэл.</param>
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
