using System;
using System.Diagnostics;
using System.IO;
using Core.Model;
using Core.Utilities;

namespace Core.Services.Implementation
{
    public class TotalCommanderManager : IExplorerManager
    {
        private readonly string _totalCommanderPath;

        public TotalCommanderManager(string totalCommanderPath)
        {
            _totalCommanderPath = Path.GetFullPath(totalCommanderPath);
        }

        public bool IsExplorer(ApplicationWindow hostWindow)
        {
            uint hostProcessId;
            WinApi.GetWindowThreadProcessId(hostWindow.Handle, out hostProcessId);

            Process hostProcess = Process.GetProcessById((int)hostProcessId);
            string executablePath = hostProcess.MainModule.FileName;

            //Ignore case is not very accurate, but should prevent possible case mistakes
            return string.Equals(executablePath, _totalCommanderPath, StringComparison.InvariantCultureIgnoreCase);
        }

        public IExplorer GetExplorer(ApplicationWindow hostWindow)
        {
            return new TotalCommander(hostWindow, _totalCommanderPath, false);
        }

        public IExplorer CreateExplorer()
        {
            return new TotalCommander(null, _totalCommanderPath, true);
        }
    }
}
