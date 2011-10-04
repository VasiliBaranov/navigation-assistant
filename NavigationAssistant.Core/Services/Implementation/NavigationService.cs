using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly IFileSystemParser _fileSystemParser;
        private readonly IMatchSearcher _matchSearcher;
        private IExplorerManager _primaryExplorerManager;
        private List<IExplorerManager> _supportedExplorerManagers;

        #endregion

        #region Constructors

        public NavigationService(IFileSystemParser fileSystemParser,
                                 IMatchSearcher matchSearcher,
                                 IExplorerManager primaryExplorerManager,
                                 List<IExplorerManager> supportedExplorerManagers)
        {
            _fileSystemParser = fileSystemParser;
            _matchSearcher = matchSearcher;
            _primaryExplorerManager = primaryExplorerManager;
            _supportedExplorerManagers = supportedExplorerManagers;
        }

        #endregion

        #region Properties

        public IFileSystemParser FileSystemParser
        {
            get { return _fileSystemParser; }
        }

        public IMatchSearcher MatchSearcher
        {
            get { return _matchSearcher; }
        }

        public IExplorerManager PrimaryExplorerManager
        {
            get { return _primaryExplorerManager; }
            set { _primaryExplorerManager = value; }
        }

        public List<IExplorerManager> SupportedExplorerManagers
        {
            get { return _supportedExplorerManagers; }
            set { _supportedExplorerManagers = value; }
        }

        #endregion

        #region Public Methods

        public List<MatchedFileSystemItem> GetFolderMatches(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return new List<MatchedFileSystemItem>();
            }

            List<FileSystemItem> folders = _fileSystemParser.GetSubFolders();
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

        public void Dispose()
        {
            _fileSystemParser.Dispose();
        }

        #endregion
    }
}
