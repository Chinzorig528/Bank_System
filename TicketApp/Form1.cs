using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace BankTicket
{
    public partial class Form1 : Form
    {
        private int currentNumber = 0;
        private string currentTicket = "A000";

        private PrintDocument printDocument = new PrintDocument();

        public Form1()
        {
            InitializeComponent();

            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        // 🔹 Дугаар авах
        private void btnGetTicket_Click(object sender, EventArgs e)
        {
            currentNumber++;

            currentTicket = "A" + currentNumber.ToString("D3");

            lblTicketNumber.Text = currentTicket;

            MessageBox.Show("Таны дугаар: " + currentTicket);
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

            e.Graphics.DrawString("Банкны дугаар", titleFont, Brushes.Black, 100, 100);
            e.Graphics.DrawString(currentTicket, ticketFont, Brushes.Black, 150, 180);
            e.Graphics.DrawString(DateTime.Now.ToString(), new Font("Arial", 10), Brushes.Black, 120, 260);
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