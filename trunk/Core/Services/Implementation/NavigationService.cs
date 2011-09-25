using System;
using System.Collections.Generic;
using Core.Model;
using Core.Utilities;

namespace Core.Services.Implementation
{
    public class NavigationService : INavigationService
    {
        private readonly IFileSystemParser _fileSystemParser;
        private readonly IMatchSearcher _matchSearcher;
        private readonly IExplorerManager _explorerManager;

        public NavigationService(IFileSystemParser fileSystemParser, IMatchSearcher matchSearcher, IExplorerManager explorerManager)
        {
            _fileSystemParser = fileSystemParser;
            _matchSearcher = matchSearcher;
            _explorerManager = explorerManager;
        }

        public List<MatchedFileSystemItem> GetFolderMatches(List<string> rootFolders, string searchText)
        {
            if (Utilities.Utility.IsNullOrEmpty(rootFolders) || string.IsNullOrEmpty(searchText))
            {
                return new List<MatchedFileSystemItem>();
            }

            List<FileSystemItem> folders = _fileSystemParser.GetFolders(rootFolders);
            List<MatchedFileSystemItem> matchedFolders = _matchSearcher.GetMatches(folders, searchText);

            return matchedFolders;
        }

        public void NavigateTo(string path, ApplicationWindow hostWindow)
        {
            IExplorer explorer = _explorerManager.IsExplorer(hostWindow)
                                     ? _explorerManager.GetExplorer(hostWindow)
                                     : _explorerManager.CreateExplorer();

            explorer.NavigateTo(path);
        }

        public ApplicationWindow GetActiveWindow()
        {
            // Obtain the handle of the active window.
            IntPtr handle = WinApi.GetForegroundWindow();

            return new ApplicationWindow(handle);
        }
    }
}
