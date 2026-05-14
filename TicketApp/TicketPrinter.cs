using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace BankTicket
{
    public class TicketPrinter
    {
        private readonly PrintDocument _printDocument;

        private string _ticketNumber = "A000";

        public TicketPrinter()
        {
            _printDocument = new PrintDocument();

            _printDocument.PrintPage += PrintPage;

            _printDocument.DefaultPageSettings.PaperSize =
                new PaperSize("Ticket", 280, 220);

            _printDocument.DefaultPageSettings.Margins =
                new Margins(5, 5, 5, 5);
        }

        public void SetTicket(string ticket)
        {
            _ticketNumber = ticket;
        }

        public void Preview()
        {
            PrintPreviewDialog preview =
                new PrintPreviewDialog();

            preview.Document = _printDocument;

            preview.ShowDialog();
        }

        private void PrintPage(
            object sender,
            PrintPageEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            Font titleFont =
                new Font("Arial", 12, FontStyle.Bold);

            Font ticketFont =
                new Font("Arial", 34, FontStyle.Bold);

            Font dateFont =
                new Font("Arial", 8);

            string title = "ТАНЫ ДУГААР";

            string date =
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss");

            int pageWidth = e.PageBounds.Width;

            SizeF titleSize =
                e.Graphics.MeasureString(
                    title,
                    titleFont);

            SizeF ticketSize =
                e.Graphics.MeasureString(
                    _ticketNumber,
                    ticketFont);

            float titleX =
                (pageWidth - titleSize.Width) / 2;

            float ticketX =
                (pageWidth - ticketSize.Width) / 2;

            e.Graphics.DrawString(
                title,
                titleFont,
                Brushes.Black,
                titleX,
                20);

            e.Graphics.DrawString(
                _ticketNumber,
                ticketFont,
                Brushes.Black,
                ticketX,
                55);

            e.Graphics.DrawString(
                date,
                dateFont,
                Brushes.Black,
                55,
                120);
        }
    }
}