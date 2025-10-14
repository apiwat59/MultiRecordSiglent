using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiRecord
{
    internal static class Program
    {
        // Disable COM interop debugging assistant
        [System.Diagnostics.DebuggerNonUserCode]
        [System.Diagnostics.DebuggerStepThrough]
        public static void SuppressComInteropWarnings()
        {
            // This method suppresses the NonComVisibleBaseClass MDA
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Suppress COM interop warnings
            SuppressComInteropWarnings();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
