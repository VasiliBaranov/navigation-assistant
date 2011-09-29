using System.Collections.Generic;
using Core.Model;

namespace Core.Services
{
    public interface INavigationService
    {
        List<string> RootFolders { get; set; }

        List<MatchedFileSystemItem> GetFolderMatches(string searchText);

        void NavigateTo(string path, ApplicationWindow hostWindow);

        ApplicationWindow GetActiveWindow();
    }
}
