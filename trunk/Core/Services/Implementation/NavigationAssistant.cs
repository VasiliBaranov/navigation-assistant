using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Core.Model;

namespace Core.Services.Implementation
{
    public class NavigationAssistant : INavigationAssistant
    {
        private readonly IFileSystemParser _fileSystemParser;
        private readonly IMatchSearcher _matchSearcher;
        private readonly IExplorerManager _explorerManager;

        public NavigationAssistant(IFileSystemParser fileSystemParser, IMatchSearcher matchSearcher, IExplorerManager explorerManager)
        {
            _fileSystemParser = fileSystemParser;
            _matchSearcher = matchSearcher;
            _explorerManager = explorerManager;
        }

        // Declare external functions.
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

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
            IExplorer explorer = _explorerManager.IsWindowExplorer(hostWindow)
                                     ? _explorerManager.GetExplorer(hostWindow)
                                     : _explorerManager.CreateExplorer();

            explorer.NavigateTo(path);
        }

        public ApplicationWindow GetActiveWindow()
        {
            // Obtain the handle of the active window.
            IntPtr handle = GetForegroundWindow();

            return new ApplicationWindow(handle);
        }
    }
}
