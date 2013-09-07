using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace BACnetInteropApp
{
    static class MainProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {



            // Ensure only 1 instance is running at a time.
            using (Mutex mutex = new Mutex(false, "47808"))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("An instance of " + Application.ProductName + " is already running");
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());

            }

        }
    }
}
