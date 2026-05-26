using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankTicket
{
    /// <summary>
    /// Bank Ticket Windows Forms app-ийн эхлэх class.
    /// WinForms-ийн харагдах байдлын үндсэн тохиргоог хийж, main ticket form-ийг нээнэ.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Аппликейшний process эхлэх үндсэн цэг.
        /// <see cref="STAThreadAttribute"/> нь Windows Forms UI component, dialog, хэвлэхтэй холбоотой feature-үүд
        /// single-threaded apartment behavior шаарддаг тул хэрэгтэй.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
