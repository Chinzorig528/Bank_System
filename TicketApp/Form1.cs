using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankTicket
{
    public partial class Form1 : Form
    {
        private readonly TicketService _ticketService;

        private readonly TicketPrinter _printer;

        private TicketResponse _currentTicket;

        public Form1()
        {
            InitializeComponent();

            HttpClient client = new HttpClient();

            client.BaseAddress =
                new Uri("http://localhost:5122/");

            _ticketService =
                new TicketService(client);

            _printer =
                new TicketPrinter();
        }

        private async void btnGetTicket_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                _currentTicket =
                    await _ticketService
                        .CreateTicketAsync();

                if (_currentTicket == null)
                {
                    MessageBox.Show(
                        "Ticket үүсгэж чадсангүй");

                    return;
                }

                lblTicketNumber.Text =
                    _currentTicket.Number;

                MessageBox.Show(
                    $"Таны дугаар: {_currentTicket.Number}");
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "Bank API ажиллахгүй байна.\n\nAPI асаагаад дахин оролдоно уу.",
                    "Холболтын алдаа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "Сервер хэт удаан хариулж байна.",
                    "Timeout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Алдаа:\n{ex.Message}",
                    "Алдаа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnPrint_Click(
            object sender,
            EventArgs e)
        {
            if (_currentTicket == null)
            {
                MessageBox.Show(
                    "Эхлээд ticket авна уу");

                return;
            }

            _printer.SetTicket(
                _currentTicket.Number);

            _printer.Preview();
        }
    }
}