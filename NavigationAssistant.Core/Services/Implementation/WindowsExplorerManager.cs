using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NavigationAssistant.Core.Model;
using SHDocVw;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class WindowsExplorerManager : IExplorerManager
    {
        private const int MaxWaitSeconds = 10;

        public bool IsExplorer(ApplicationWindow hostWindow)
        {
            InternetExplorer windowsExplorer = GetWindowsExplorer(hostWindow);
            return windowsExplorer != null;
        }

        public IExplorer GetExplorer(ApplicationWindow hostWindow)
        {
            InternetExplorer windowsExplorer = GetWindowsExplorer(hostWindow);

            return new WindowsExplorer(windowsExplorer);
        }

        public IExplorer CreateExplorer()
        {
            InternetExplorer windowsExplorer = CreateNewExplorer();

            return new WindowsExplorer(windowsExplorer);
        }

        private InternetExplorer CreateNewExplorer()
        {
            //We could open a new explorer at the correct location according to http://support.microsoft.com/kb/130510
            //Process.Start("explorer.exe", "/n,/root," + "file:///" + Path.GetFullPath(path));
            //But it's impossible to use this method, as in this case subsequent calls to Navigate methods of the explorer window 
            //will throw exceptions (probably as the root is non-standard).

            List<InternetExplorer> initialExplorers = GetAllExplorers();

            //This process will exit as soon as it starts a new windows explorer window,
            //so we have to search for the actual explorer window.
            //Note that actually all of the explorer windows are hosted inside a default explorer process (started at windows start),
            //so we can not even iterate over processes.
            Process.Start("explorer.exe");

            List<InternetExplorer> newExplorers = GetNewExplorers(initialExplorers);

            InternetExplorer createdExplorer = GetCreatedExplorer(initialExplorers, newExplorers);

            return createdExplorer;
        }

        private InternetExplorer GetCreatedExplorer(List<InternetExplorer> initialExplorers, List<InternetExplorer> newExplorers)
        {
            IEnumerable<int> initialHandles = initialExplorers.Select(ie => ie.HWND);
            IEnumerable<int> newHandles = newExplorers.Select(ie => ie.HWND);

            List<int> addedHandles = newHandles.Except(initialHandles).ToList();

            if (addedHandles.Count == 0)
            {
                throw new InvalidOperationException("Windows explorer could not be created");
            }

            if (addedHandles.Count > 1)
            {
                throw new InvalidOperationException("Can not determine the created explorer");
            }

            int createdWindowHandle = addedHandles[0];

            InternetExplorer createdExplorer = newExplorers.First(e => e.HWND == createdWindowHandle);

            return createdExplorer;
        }

        private List<InternetExplorer> GetNewExplorers(List<InternetExplorer> initialExplorers)
        {
            List<InternetExplorer> newExplorers = GetAllExplorers();

            //Wait
            TimeSpan maxExecutionTime = new TimeSpan(0, 0, MaxWaitSeconds);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (newExplorers.Count <= initialExplorers.Count)
            {
                newExplorers = GetAllExplorers();
                TimeSpan executionTime = stopwatch.Elapsed;

                if (executionTime > maxExecutionTime)
                {
                    break;
                }
            }

            stopwatch.Stop();

            return newExplorers;
        }

        private InternetExplorer GetWindowsExplorer(ApplicationWindow expectedWindow)
        {
            List<InternetExplorer> explorers = GetAllExplorers();

            InternetExplorer correctExplorer = explorers.FirstOrDefault(e => IsExpectedWindow(e, expectedWindow));

            return correctExplorer;
        }

        private bool IsExpectedWindow(InternetExplorer actualWindow, ApplicationWindow expectedWindow)
        {
            IntPtr actualHandle = new IntPtr(actualWindow.HWND);
            bool windowHandleIsCorrect = expectedWindow != null && actualHandle == expectedWindow.Handle;

            return windowHandleIsCorrect;
        }

        //Code taken from http://omegacoder.com/?p=63
        private List<InternetExplorer> GetAllExplorers()
        {
            ShellWindows shellWindows = new ShellWindowsClass();

            List<InternetExplorer> explorers = shellWindows.OfType<InternetExplorer>().Where(IsWindowsExplorer).ToList();

            return explorers;
        }

        private bool IsWindowsExplorer(InternetExplorer ie)
        {
            string filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower();

            return filename.Equals("explorer");
        }
    }
}
