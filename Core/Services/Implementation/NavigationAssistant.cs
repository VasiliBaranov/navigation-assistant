using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Core.Model;
using SHDocVw;

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

        public void NavigateTo(string path)
        {
            InternetExplorer windowsExplorer = GetWindowsExplorer();

            if (windowsExplorer != null)
            {
                windowsExplorer.Navigate("file:///" + Path.GetFullPath(path));
            }
            else
            {
                //Open new explorer; see http://support.microsoft.com/kb/130510
                //Process.Start("explorer.exe", "/e,/root," + Path.GetFullPath(path));

                //Can not use the method above as in this case subsequent calls to Navigate methods of the window throw esception
                //(probably as root is non-standard).
                Process.Start("explorer.exe");

                //Wait
                while (windowsExplorer == null)
                {
                    windowsExplorer = GetWindowsExplorer();
                }

                windowsExplorer.Navigate("file:///" + Path.GetFullPath(path));
            }
        }

        //Code taken from http://omegacoder.com/?p=63
        private InternetExplorer GetWindowsExplorer()
        {
            ShellWindows shellWindows = new ShellWindowsClass();

            foreach (InternetExplorer ie in shellWindows)
            {
                string filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower();

                //IE name is iexplore, Windows Explorer name is explorer
                if (filename.Equals("explorer"))
                {
                    return ie;
                }
            }

            return null;
        }
    }
}
