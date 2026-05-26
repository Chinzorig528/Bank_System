using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QueueDesktop
{
    /// <summary>
    /// QueueDesktop application-ийн эхлэх цэг.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// WinForms application-г эхлүүлж үндсэн form-ийг нээнэ.
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
