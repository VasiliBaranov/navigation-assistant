using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines methods for searching for matches in the file system items cache according to the query.
    /// </summary>
    public interface IMatchSearcher
    {
        List<MatchedFileSystemItem> GetMatches(List<FileSystemItem> items, string searchText);
    }
}
