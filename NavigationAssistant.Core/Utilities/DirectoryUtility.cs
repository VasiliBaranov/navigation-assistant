using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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
            if (string.IsNullOrEmpty(path))
            {
                return new List<string>();
            }

            path = Path.GetFullPath(path);

            List<string> folders = path
                .Split(new[] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            return folders;
        }
    }
}
