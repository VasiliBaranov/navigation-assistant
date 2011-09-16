using System.Collections.Generic;
using Core.Model;

namespace Core.Services
{
    public interface INavigationAssistant
    {
        List<MatchedFileSystemItem> GetFolders(string rootPath, string match);
    }
}
