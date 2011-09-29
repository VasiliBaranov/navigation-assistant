using System.Collections.Generic;
using Core.Model;

namespace Core.Services
{
    public interface IFileSystemParser
    {
        bool IncludeHiddenFolders { get; set; }

        List<string> ExcludeFolderTemplates { get; set; }

        List<string> FoldersToParse { get; set; }

        List<FileSystemItem> GetSubFolders();
    }
}
