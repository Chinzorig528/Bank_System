using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankTicket
{
    /// <summary>
    /// Ticket олгох application-ийн үндсэн Windows Forms дэлгэц.
    /// Энэ form нь хэрэглэгчид Bank API-аас шинэ queue ticket авах, ticket дугаарыг харах,
    /// хэвлэх боломжтой PDF copy хадгалах, хадгалсан PDF-ийг нээж шалгах боломж өгнө.
    /// </summary>
    public partial class Form1 : Form
    {
        private readonly TicketService _ticketService;

        private readonly TicketPrinter _printer;

        private TicketResponse _currentTicket;

        /// <summary>
        /// Үндсэн form-ийг үүсгэж, API client-ийг тохируулж, хэвлэгчийн туслах class-ийг бэлдэнэ.
        /// API base address нь <c>Bank.API</c> host хийж байгаа computer рүү заана.
        /// Ticket үүсгэх бүх request <see cref="TicketService"/>-ээр дамжин явна.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            HttpClient client = new HttpClient();

            client.BaseAddress =
                new Uri("http://192.168.88.6:5092/");

            _ticketService =
                new TicketService(client);

            _printer =
                new TicketPrinter();
        }

        /// <summary>
        /// "Get Ticket" button дарагдах үед ажиллана.
        /// API-аас шинэ queue ticket үүсгүүлж, буцаж ирсэн ticket дугаарыг дэлгэц дээр харуулна.
        /// Мөн тухайн ticket-ийг <see cref="_currentTicket"/> дотор хадгалснаар дараа нь print button ижил ticket-ийг ашиглана.
        /// </summary>
        /// <param name="sender">
        /// Event үүсгэсэн button instance.
        /// </param>
        /// <param name="e">
        /// Button click-ийн стандарт event data.
        /// </param>
        /// <remarks>
        /// Network болон timeout exception-уудыг энд барьж хэрэглэгчид ойлгомжтой message болгон харуулна.
        /// Ингэснээр desktop app crash хийхгүй, хэрэглэгч API асаах эсвэл дахин оролдох хэрэгтэйгээ ойлгоно.
        /// </remarks>
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

        /// <summary>
        /// "Print" button дарагдах үед ажиллана.
        /// Эхлээд ticket үүссэн эсэхийг шалгаад, одоогийн ticket-ийг PDF файл болгон хадгална.
        /// Дараа нь хадгалсан файлыг system-ийн default PDF viewer-ээр нээнэ.
        /// </summary>
        /// <param name="sender">
        /// Event үүсгэсэн button instance.
        /// </param>
        /// <param name="e">
        /// Button click-ийн стандарт event data.
        /// </param>
        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (_currentTicket == null)
            {
                MessageBox.Show("Эхлээд ticket авна уу");
                return;
            }

            _printer.SetTicket(
    _currentTicket.Number);

            string savedPath =
                _printer.PrintToPdfFile();

            MessageBox.Show(
                $"PDF файл хадгалагдлаа:\n\n{savedPath}",
                "Амжилттай",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = savedPath,
                    UseShellExecute = true
                });
        }
    }
}
