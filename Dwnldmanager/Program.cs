using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Dwnldmanager
{
    static class Program
    {
        public static Form1 form1;
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(form1 = new Form1());
        }
    }
}
