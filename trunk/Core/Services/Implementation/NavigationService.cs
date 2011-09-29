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
        private readonly IExplorerManager _primaryExplorerManager;
        private readonly List<IExplorerManager> _supportedExplorerManagers;

        private List<string> _rootFolders;

        public NavigationService(IFileSystemParser fileSystemParser, 
            IMatchSearcher matchSearcher, 
            IExplorerManager primaryExplorerManager,
            List<IExplorerManager> supportedExplorerManagers)
        {
            _fileSystemParser = fileSystemParser;
            _matchSearcher = matchSearcher;
            _primaryExplorerManager = primaryExplorerManager;
            _supportedExplorerManagers = supportedExplorerManagers;

            _rootFolders = DirectoryUtility.GetHardDriveRootFolders();
        }

        //public List<string> RootFolders { get; set; }
        public List<string> RootFolders
        {
            get
            {
                return _rootFolders;
            }
            set
            {
                _rootFolders = value;
                if (ListUtility.IsNullOrEmpty(_rootFolders))
                {
                    _rootFolders = DirectoryUtility.GetHardDriveRootFolders();
                }
            }
        }

        public List<MatchedFileSystemItem> GetFolderMatches(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return new List<MatchedFileSystemItem>();
            }

            List<FileSystemItem> folders = _fileSystemParser.GetSubFolders(_rootFolders);
            List<MatchedFileSystemItem> matchedFolders = _matchSearcher.GetMatches(folders, searchText);

            return matchedFolders;
        }

        public void NavigateTo(string path, ApplicationWindow hostWindow)
        {
            IExplorer explorer = null;
            foreach (IExplorerManager explorerManager in _supportedExplorerManagers)
            {
                if (explorerManager.IsExplorer(hostWindow))
                {
                    explorer = explorerManager.GetExplorer(hostWindow);
                    break;
                }
            }

            if (explorer == null)
            {
                explorer = _primaryExplorerManager.CreateExplorer();
            }

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
