using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Utilities
{
    public static class DirectoryUtility
    {
        public static List<string> GetHardDriveRootFolders()
        {
            return
                DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Fixed && d.IsReady)
                .Select(d => d.RootDirectory.FullName)
                .ToList();
        }

        public static void EnsureClearFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                try
                {
                    Directory.Delete(folderPath, true);
                }
                catch
                {
                    // TODO: aparently we have a racing condition, for now we sleep and try again
                    Thread.Sleep(1000);
                    Directory.Delete(folderPath, true);
                }
            }
            Directory.CreateDirectory(folderPath);
        }

        public static void EnsureFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public static List<string> SplitPath(string path)
        {
            path = Path.GetFullPath(path);

            List<string> folders = path
                .Split(new[] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            return folders;
        }

        public static FileSystemItem GetFileSystemItem(DirectoryInfo directoryInfo)
        {
            return new FileSystemItem(directoryInfo.FullName)
                       {
                           IsHidden = directoryInfo.IsHidden()
                       };
        }

        public static bool IsHidden(this DirectoryInfo directoryInfo)
        {
            return (directoryInfo.Attributes & FileAttributes.Hidden) != 0;
        }

        public static FileSystemItem FindItem(List<FileSystemItem> items, string fullPath)
        {
            FileSystemItem item = items.FirstOrDefault(i => String.Equals(i.FullPath, fullPath, StringComparison.Ordinal));
            return item;
        }
    }
}
