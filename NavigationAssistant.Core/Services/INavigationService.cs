using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface INavigationService : IDisposable
    {
        List<MatchedFileSystemItem> GetFolderMatches(string searchText);

        void NavigateTo(string path, ApplicationWindow hostWindow);

        ApplicationWindow GetActiveWindow();

        IFileSystemParser FileSystemParser { get; }

        IMatchSearcher MatchSearcher { get; }

        INavigatorManager PrimaryNavigatorManager { get; set; }

        List<INavigatorManager> SupportedNavigatorManagers { get; set; }
    }
}
