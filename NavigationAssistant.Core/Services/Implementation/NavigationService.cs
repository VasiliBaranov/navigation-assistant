using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services.Implementation
{
    /// <summary>
    /// Implements a high-level navigation assistance (facade).
    /// </summary>
    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly IFileSystemParser _fileSystemParser;
        private readonly IMatchSearcher _matchSearcher;
        private INavigatorManager _primaryNavigatorManager;
        private List<INavigatorManager> _supportedNavigatorManagers;

        #endregion

        #region Constructors

        public NavigationService(IFileSystemParser fileSystemParser,
                                 IMatchSearcher matchSearcher,
                                 INavigatorManager primaryNavigatorManager,
                                 List<INavigatorManager> supportedNavigatorManagers)
        {
            _fileSystemParser = fileSystemParser;
            _matchSearcher = matchSearcher;
            _primaryNavigatorManager = primaryNavigatorManager;
            _supportedNavigatorManagers = supportedNavigatorManagers;
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

        public INavigatorManager PrimaryNavigatorManager
        {
            get { return _primaryNavigatorManager; }
            set { _primaryNavigatorManager = value; }
        }

        public List<INavigatorManager> SupportedNavigatorManagers
        {
            get { return _supportedNavigatorManagers; }
            set { _supportedNavigatorManagers = value; }
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
            if (_supportedNavigatorManagers == null || _primaryNavigatorManager == null)
            {
                throw new InvalidOperationException("Please specify supported navigator managers and primary navigator manager.");
            }

            INavigator navigator = null;
            if (hostWindow != null)
            {
                foreach (INavigatorManager navigatorManager in _supportedNavigatorManagers)
                {
                    if (navigatorManager.IsNavigator(hostWindow))
                    {
                        navigator = navigatorManager.GetNavigator(hostWindow);
                        break;
                    }
                }
            }

            if (navigator == null)
            {
                navigator = _primaryNavigatorManager.CreateNavigator();
            }

            navigator.NavigateTo(path);
        }

        public void Dispose()
        {
            _fileSystemParser.Dispose();
        }

        #endregion
    }
}
