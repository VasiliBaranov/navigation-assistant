using System;
using System.Diagnostics;
using System.IO;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
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
            WinApi.GetWindowThreadProcessId(hostWindow.Handle, out hostProcessId);

            Process hostProcess = Process.GetProcessById((int)hostProcessId);
            string executablePath = hostProcess.MainModule.FileName;

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
    }
}
