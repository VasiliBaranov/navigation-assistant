using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace ConsoleExplorerClient
{
    internal class Program
    {
        // Declare external functions.
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static void Main(string[] args)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindowsClass();

            string filename;

            foreach (SHDocVw.InternetExplorer ie in shellWindows)
            {
                filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower();

                if (filename.Equals("iexplore"))
                {
                    Console.WriteLine("Web Site   : {0}", ie.LocationURL);
                }

                if (filename.Equals("explorer"))
                {
                    Console.WriteLine("Hard Drive : {0}", ie.LocationURL);

                    ie.Navigate("file:///e:\\temp\\my folder\\");

                    Thread.Sleep(1000);
                    Console.WriteLine("Hard Drive : {0}", ie.LocationURL);
                }
            }

            int chars = 256;
            StringBuilder buff = new StringBuilder(chars);

            // Obtain the handle of the active window.
            IntPtr handle = GetForegroundWindow();

            // Update the controls.
            if (GetWindowText(handle, buff, chars) > 0)
            {
                Console.WriteLine(buff.ToString());
                Console.WriteLine(handle.ToString());
            }

            Console.ReadKey();
        }
    }
}
