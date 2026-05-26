using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QueueDisplayWinForms
{
    /// <summary>
    /// QueueDisplayWinForms application-ийн эхлэх цэг.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// WinForms application-г эхлүүлж үндсэн display form-ийг нээнэ.
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
