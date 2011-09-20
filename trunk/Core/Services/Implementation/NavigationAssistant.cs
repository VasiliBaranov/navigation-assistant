using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Core.Model;
using SHDocVw;
using System.Linq;

namespace Core.Services.Implementation
{
    public class NavigationAssistant : INavigationAssistant
    {
        private readonly IFileSystemParser _fileSystemParser;
        private readonly IMatchSearcher _matchSearcher;

        public NavigationAssistant(IFileSystemParser fileSystemParser, IMatchSearcher matchSearcher)
        {
            _fileSystemParser = fileSystemParser;
            _matchSearcher = matchSearcher;
        }

        // Declare external functions.
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public List<MatchedFileSystemItem> GetFolderMatches(List<string> rootFolders, string searchText)
        {
            if (Utilities.IsNullOrEmpty(rootFolders) || string.IsNullOrEmpty(searchText))
            {
                return new List<MatchedFileSystemItem>();
            }

            List<FileSystemItem> folders = _fileSystemParser.GetFolders(rootFolders);
            List<MatchedFileSystemItem> matchedFolders = _matchSearcher.GetMatches(folders, searchText);

            return matchedFolders;
        }

        public void NavigateTo(string path, ApplicationWindow hostWindow)
        {
            InternetExplorer windowsExplorer = GetWindowsExplorer(hostWindow);

            if (windowsExplorer != null)
            {
                windowsExplorer.Navigate("file:///" + Path.GetFullPath(path));
            }
            else
            {
                //Open new explorer; see http://support.microsoft.com/kb/130510
                //Process.Start("explorer.exe", "/n,/root," + "file:///" + Path.GetFullPath(path));

                List<InternetExplorer> initialExplorers = GetAllExplorers();

                //Can not use the method above as in this case subsequent calls to Navigate methods of the window 
                //will throw exceptions (probably as the root is non-standard).
                Process process = Process.Start("explorer.exe");

                //The process above exits as soon as it starts new windows explorer window,
                //so we have to search for the actual explorer window again.
                //Note that actually all of the explorer windows are hosted inside a default explorer process (started at windows start),
                //so we can not even iterate over processes.

                List<InternetExplorer> newExplorers = GetAllExplorers();

                //Wait
                TimeSpan maxExecutionTime = new TimeSpan(0, 0, 10);
                TimeSpan executionTime;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                while (newExplorers.Count <= initialExplorers.Count)
                {
                    newExplorers = GetAllExplorers();
                    executionTime = stopwatch.Elapsed;

                    if (executionTime > maxExecutionTime)
                    {
                        break;
                    }
                }

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

                InternetExplorer createdWindow = newExplorers.First(e => e.HWND == createdWindowHandle);

                createdWindow.Navigate("file:///" + Path.GetFullPath(path));
            }
        }

        public ApplicationWindow GetActiveWindow()
        {
            // Obtain the handle of the active window.
            IntPtr handle = GetForegroundWindow();

            // Update the controls.
            int chars = 256;
            StringBuilder buff = new StringBuilder(chars);
            GetWindowText(handle, buff, chars);

            return new ApplicationWindow(handle);
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
            uint currentProcessId;
            GetWindowThreadProcessId(actualHandle, out currentProcessId);

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
