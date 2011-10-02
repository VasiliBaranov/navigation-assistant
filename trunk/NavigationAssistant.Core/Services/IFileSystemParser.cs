using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface IFileSystemParser
    {
        bool IncludeHiddenFolders { get; set; }

        List<string> ExcludeFolderTemplates { get; set; }

        List<string> FoldersToParse { get; set; }

        List<FileSystemItem> GetSubFolders();
    }
}
