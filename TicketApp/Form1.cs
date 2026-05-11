using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankTicket
{
    public partial class Form1 : Form
    {
        private string currentTicket = "A000";

        private readonly PrintDocument printDocument = new PrintDocument();
        private readonly HttpClient client = new HttpClient();

        public Form1()
        {
            InitializeComponent();

            printDocument.PrintPage += PrintDocument_PrintPage;

            // Ticket printer-ийн жижиг цаасны хэмжээ
            printDocument.DefaultPageSettings.PaperSize = new PaperSize("Ticket", 280, 220);
            printDocument.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);


            // Bank.API-ийн URL
            client.BaseAddress = new Uri("http://localhost:5122/");

        }

        // API-аас дугаар авах
        private async void btnGetTicket_Click(object sender, EventArgs e)
        {
            try
            {
                // Bank.API дээрх POST /ticket endpoint руу хүсэлт явуулна
                HttpResponseMessage response = await client.PostAsync("ticket", null);

                if (response.IsSuccessStatusCode)
                {
                    string ticketNumber = await response.Content.ReadAsStringAsync();

                    currentTicket = ticketNumber.Trim('"');
                    lblTicketNumber.Text = currentTicket;

                    MessageBox.Show("Таны дугаар: " + currentTicket);
                }
                else
                {
                    MessageBox.Show("Дугаар авахад алдаа гарлаа.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bank.API ажиллаагүй байна.\n\n" + ex.Message);
            }
        }

        // 🔹 Хэвлэх
        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (currentTicket == "A000")
            {
                MessageBox.Show("Эхлээд дугаар авна уу!");
                return;
            }

            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = printDocument;
            preview.ShowDialog();
        }

        // 🔹 Хэвлэх загвар
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            Font titleFont = new Font("Arial", 12, FontStyle.Bold);
            Font ticketFont = new Font("Arial", 34, FontStyle.Bold);
            Font dateFont = new Font("Arial", 8);
            Font infoFont = new Font("Arial", 8);

            int pageWidth = e.PageBounds.Width;

            string title = "ТАНЫ ДУГААР";
            string ticket = currentTicket;
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string info = "Та дугаараа хүлээнэ үү";

            // Текстийн өргөнийг хэмжиж төвд байрлуулах
            SizeF titleSize = e.Graphics.MeasureString(title, titleFont);
            SizeF ticketSize = e.Graphics.MeasureString(ticket, ticketFont);
            SizeF dateSize = e.Graphics.MeasureString(date, dateFont);
            SizeF infoSize = e.Graphics.MeasureString(info, infoFont);

            float titleX = (pageWidth - titleSize.Width) / 2;
            float ticketX = (pageWidth - ticketSize.Width) / 2;
            float dateX = (pageWidth - dateSize.Width) / 2;
            float infoX = (pageWidth - infoSize.Width) / 2;

            e.Graphics.DrawString(title, titleFont, Brushes.Black, titleX, 20);
            e.Graphics.DrawString(ticket, ticketFont, Brushes.Black, ticketX, 55);
            e.Graphics.DrawString(date, dateFont, Brushes.Black, dateX, 120);
            e.Graphics.DrawString(info, infoFont, Brushes.Black, infoX, 145);

            // Доод тасархай зураас
            e.Graphics.DrawString("-------------------------------------------------------", infoFont, Brushes.Black, 35, 170);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Label дээр дарахад юу ч хийхгүй
        }

        private void btnGetTicket_Click_1(object sender, EventArgs e)
        {
            btnGetTicket_Click(sender, e);
        }
    }
}