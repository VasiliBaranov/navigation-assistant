using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface IMatchSearcher
    {
        List<MatchedFileSystemItem> GetMatches(List<FileSystemItem> items, string searchText);
    }
}
