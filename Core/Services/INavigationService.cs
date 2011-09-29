using System.Collections.Generic;
using Core.Model;

namespace Core.Services
{
    public interface INavigationService
    {
        List<MatchedFileSystemItem> GetFolderMatches(string searchText);

        void NavigateTo(string path, ApplicationWindow hostWindow);

        ApplicationWindow GetActiveWindow();
    }
}
