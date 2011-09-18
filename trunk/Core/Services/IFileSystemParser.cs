using System.Collections.Generic;
using Core.Model;

namespace Core.Services
{
    public interface IFileSystemParser
    {
        List<FileSystemItem> GetFolders(List<string> rootFolders);
    }
}
