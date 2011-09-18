using System.Collections.Generic;
using Core.Model;

namespace Core.Services.Implementation
{
    public class NavigationAssistant : INavigationAssistant
    {
        private readonly IFileSystemParser _fileSystemParser;
        private readonly IMatchSearcher _matchSearcher;

        public NavigationAssistant(IFileSystemParser fileSystemParser, IMatchSearcher matchSearcher)
        {
            _fileSystemParser = fileSystemParser;
            _matchSearcher = matchSearcher;
        }

        public List<MatchedFileSystemItem> GetFolderMatches(List<string> rootFolders, string searchText)
        {
            if (Utilities.IsNullOrEmpty(rootFolders) || string.IsNullOrEmpty(searchText))
            {
                return new List<MatchedFileSystemItem>();
            }

            List<FileSystemItem> folders = _fileSystemParser.GetFolders(rootFolders);
            List<MatchedFileSystemItem> matchedFolders = _matchSearcher.GetMatches(folders, searchText);

            return matchedFolders;
        }
    }
}
