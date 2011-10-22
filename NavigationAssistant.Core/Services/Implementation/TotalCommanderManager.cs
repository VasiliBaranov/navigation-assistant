using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services.Implementation
{
    /// <summary>
    /// Implements a manager for total commander windows.
    /// </summary>
    public class TotalCommanderManager : INavigatorManager
    {
        private readonly string _totalCommanderPath;

        public TotalCommanderManager(string totalCommanderPath)
        {
            _totalCommanderPath = Path.GetFullPath(totalCommanderPath);
        }

        public bool IsNavigator(ApplicationWindow hostWindow)
        {
            uint hostProcessId;
            GetWindowThreadProcessId(hostWindow.Handle, out hostProcessId);

            Process hostProcess = Process.GetProcessById((int)hostProcessId);

            string executablePath = null;
            try
            {
                executablePath = hostProcess.MainModule.FileName;
            }
            catch (Exception)
            {
                //This may happen if hostProcess is not TotalCommander (e.g. Windows Explorer)
            }

            //Ignore case is not very accurate, but should prevent possible case mistakes
            return string.Equals(executablePath, _totalCommanderPath, StringComparison.InvariantCultureIgnoreCase);
        }

        public INavigator GetNavigator(ApplicationWindow hostWindow)
        {
            return new TotalCommander(hostWindow, _totalCommanderPath, false);
        }

        public INavigator CreateNavigator()
        {
            return new TotalCommander(null, _totalCommanderPath, true);
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    }
}
