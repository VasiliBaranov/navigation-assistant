using System.Collections.Generic;
using System.IO;
using Core.Model;
using System.Linq;

namespace Core.Services.Implementation
{
    public class FileSystemParser : IFileSystemParser
    {
        public List<FileSystemItem> GetFolders(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                return new List<FileSystemItem>();
            }

            string[] folders = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);

            List<FileSystemItem> result = folders.Select(GetFileSystemInfo).ToList();
            result.Add(GetFileSystemInfo(rootPath));

            return result;
        }

        private static FileSystemItem GetFileSystemInfo(string path)
        {
            return new FileSystemItem { ItemName = Path.GetFileName(path), ItemPath = path };
        }
    }
}
