using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.IO;

namespace BankTicket
{
    /// <summary>
    /// Хэрэглэгчийн queue ticket-ийг бэлдэж хэвлэх үүрэгтэй class.
    /// Энэ class нь <see cref="PrintDocument"/> тохиргоо, ticket page layout, print preview,
    /// бодит хэвлэх үйлдэл болон Windows-ийн <c>Microsoft Print to PDF</c> printer-ээр PDF гаргах logic-ийг хариуцна.
    /// </summary>
    public class TicketPrinter
    {
        private readonly PrintDocument _printDocument;

        private string _ticketNumber = "A000";

        /// <summary>
        /// Одоогоор тохируулсан ticket-ийг default printer рүү илгээнэ.
        /// <see cref="StandardPrintController"/> ашигласнаар Windows-ийн хэвлэх progress dialog харуулахгүйгээр хэвлэнэ.
        /// </summary>
        /// <remarks>
        /// Хэвлэгдэх ticket дээр хамгийн сүүлийн queue дугаар гарах ёстой тул энэ method-оос өмнө
        /// <see cref="SetTicket"/>-ийг дуудах хэрэгтэй.
        /// </remarks>
        public void Print()
        {
            _printDocument.PrintController =
                new StandardPrintController();

            _printDocument.Print();
        }

        /// <summary>
        /// Queue slip-д тохиромжтой жижиг paper size-тай ticket printer үүсгэнэ.
        /// Мөн document хэвлэгдэх эсвэл preview харагдах бүрт ticket title, queue дугаар, timestamp зурдаг
        /// <see cref="PrintPage"/> event handler-ийг холбоно.
        /// </summary>
        public TicketPrinter()
        {
            _printDocument = new PrintDocument();

            _printDocument.PrintPage += PrintPage;

            _printDocument.DefaultPageSettings.PaperSize =
                new PaperSize("Ticket", 280, 220);

            _printDocument.DefaultPageSettings.Margins =
                new Margins(5, 5, 5, 5);
        }

        /// <summary>
        /// Хэвлэх, preview харах, эсвэл PDF болгоход ашиглах ticket дугаарыг тохируулна.
        /// </summary>
        /// <param name="ticket">
        /// Ticket дээр харуулах queue дугаар. Жишээ нь <c>A001</c>.
        /// </param>
        public void SetTicket(string ticket)
        {
            _ticketNumber = ticket;
        }

        /// <summary>
        /// Одоогийн ticket layout-ийг Windows print preview dialog дээр нээнэ.
        /// Printer рүү илгээхээс өмнө ticket ямар харагдахыг шалгахад хэрэгтэй.
        /// </summary>
        public void Preview()
        {
            PrintPreviewDialog preview =
                new PrintPreviewDialog();

            preview.Document = _printDocument;

            preview.ShowDialog();
        }

        /// <summary>
        /// Одоогийн ticket-ийг Windows-ийн <c>Microsoft Print to PDF</c> printer ашиглан PDF файл болгож хадгална.
        /// Үүссэн PDF нь application executable-ийн хажууд байрлах <c>Tickets</c> folder дотор хадгалагдана.
        /// </summary>
        /// <returns>
        /// Үүсгэсэн PDF файлын absolute path.
        /// </returns>
        /// <remarks>
        /// File name дотор ticket дугаар болон timestamp ордог тул олон ticket хадгалахад өмнөх файлууд дарж бичигдэхгүй.
        /// </remarks>
        public string PrintToPdfFile()
        {
            string appFolder =
                Application.StartupPath;

            string ticketsFolder =
                Path.Combine(appFolder, "Tickets");

            if (!Directory.Exists(ticketsFolder))
            {
                Directory.CreateDirectory(ticketsFolder);
            }

            string fileName =
                $"Ticket_{_ticketNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            string filePath =
                Path.Combine(ticketsFolder, fileName);

            _printDocument.PrinterSettings.PrinterName =
                "Microsoft Print to PDF";

            _printDocument.PrinterSettings.PrintToFile =
                true;

            _printDocument.PrinterSettings.PrintFileName =
                filePath;

            _printDocument.PrintController =
                new StandardPrintController();

            _printDocument.Print();

            return filePath;
        }

        /// <summary>
        /// Print page дээр ticket-ийн content-ийг зурна.
        /// Layout нь title болон ticket дугаарыг голлуулж, доор нь одоогийн timestamp-ийг нэмдэг.
        /// </summary>
        /// <param name="sender">
        /// Event үүсгэсэн print document.
        /// </param>
        /// <param name="e">
        /// Зурах graphics surface болон page metadata агуулсан print page event argument.
        /// </param>
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
            e.HasMorePages = false;
        }
    }
}
