using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tril.App
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="filesToOpen"></param>
        [STAThread]
        static void Main(string[] filesToOpen)
        {
            FilesToOpen = filesToOpen;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WfaMainWin());
        }

        internal static string[] FilesToOpen;
    }
}
