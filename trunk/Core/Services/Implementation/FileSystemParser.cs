using System;
using System.Collections.Generic;
using System.IO;
using Core.Model;
using System.Linq;

namespace Core.Services.Implementation
{
    public class FileSystemParser : IFileSystemParser
    {
        public List<FileSystemItem> GetFolders(List<string> rootFolders)
        {
            if (Utilities.Utilities.IsNullOrEmpty(rootFolders))
            {
                throw new ArgumentNullException("rootFolders");
            }

            List<string> folders = new List<string>();

            foreach (string rootFolder in rootFolders)
            {
                folders.AddRange(Utilities.Utilities.GetFoldersRecursively(rootFolder));
                folders.Add(rootFolder);
            }

            List<FileSystemItem> result = folders.Select(GetFileSystemInfo).ToList();

            return result;
        }

        private static FileSystemItem GetFileSystemInfo(string path)
        {
            return new FileSystemItem(Path.GetFileName(path), path);
        }
    }
}
