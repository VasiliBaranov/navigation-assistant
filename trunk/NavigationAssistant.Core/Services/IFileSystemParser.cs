using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface IFileSystemParser : IDisposable
    {
        List<string> ExcludeFolderTemplates { get; set; }

        List<string> FoldersToParse { get; set; }

        List<FileSystemItem> GetSubFolders();
    }
}
