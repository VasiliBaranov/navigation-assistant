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

        public List<MatchedFileSystemItem> GetFolders(string rootPath, string match)
        {
            if (string.IsNullOrEmpty(rootPath) || string.IsNullOrEmpty(match))
            {
                return new List<MatchedFileSystemItem>();
            }

            List<FileSystemItem> folders = _fileSystemParser.GetFolders(rootPath);
            List<MatchedFileSystemItem> matchedFolders = _matchSearcher.GetMatches(folders, match);

            return matchedFolders;
        }
    }
}
