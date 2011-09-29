using System;
using System.Collections.Generic;
using System.IO;
using Core.Model;
using System.Linq;
using Core.Utilities;

namespace Core.Services.Implementation
{
    public class FileSystemParser : IFileSystemParser
    {
        public List<FileSystemItem> GetSubFolders(List<string> rootFolders)
        {
            if (ListUtility.IsNullOrEmpty(rootFolders))
            {
                throw new ArgumentNullException("rootFolders");
            }

            List<string> folders = new List<string>();

            foreach (string rootFolder in rootFolders)
            {
                folders.AddRange(DirectoryUtility.GetFoldersRecursively(rootFolder));
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
