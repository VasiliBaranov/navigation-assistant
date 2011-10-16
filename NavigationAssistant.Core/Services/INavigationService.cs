using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines high-level methods and properties for assisting navigation.
    /// </summary>
    public interface INavigationService : IDisposable
    {
        #region Properties

        IFileSystemParser FileSystemParser { get; }

        IMatchSearcher MatchSearcher { get; }

        INavigatorManager PrimaryNavigatorManager { get; set; }

        List<INavigatorManager> SupportedNavigatorManagers { get; set; }

        #endregion

        ApplicationWindow GetActiveWindow();

        List<MatchedFileSystemItem> GetFolderMatches(string searchText);

        void NavigateTo(string path, ApplicationWindow hostWindow);
    }
}
