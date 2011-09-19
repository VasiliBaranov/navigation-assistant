using System.Collections.Generic;
using Core.Model;

namespace Core.Services
{
    public interface INavigationAssistant
    {
        List<MatchedFileSystemItem> GetFolderMatches(List<string> rootFolders, string searchText);

        void NavigateTo(string path);
    }
}
