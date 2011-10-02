using System.Collections.Generic;
using Core.Model;

namespace Core.Services
{
    public interface IMatchSearcher
    {
        List<MatchedFileSystemItem> GetMatches(List<FileSystemItem> items, string searchText);
    }
}
