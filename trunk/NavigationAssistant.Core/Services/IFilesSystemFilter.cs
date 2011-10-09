using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface IFileSystemFilter
    {
        List<string> ExcludeFolderTemplates { get; set; }

        List<string> FoldersToParse { get; set; }

        bool IsCorrect(FileSystemItem item);

        List<FileSystemItem> FilterItems(List<FileSystemItem> items);
    }
}
