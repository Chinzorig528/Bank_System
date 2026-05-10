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

                    currentTicket = ticketNumber;
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
            Font titleFont = new Font("Arial", 18, FontStyle.Bold);
            Font ticketFont = new Font("Arial", 36, FontStyle.Bold);
            Font dateFont = new Font("Arial", 10);

            e.Graphics.DrawString("Банкны дугаар", titleFont, Brushes.Black, 100, 100);
            e.Graphics.DrawString(currentTicket, ticketFont, Brushes.Black, 150, 180);
            e.Graphics.DrawString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), dateFont, Brushes.Black, 120, 260);
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