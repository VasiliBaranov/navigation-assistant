using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NavigationAssistant.Core.Utilities
{
    public static class DirectoryUtility
    {
        public static List<string> GetFoldersRecursively(string folderPath)
        {
            List<string> subfolders = new List<string>();
            GetFoldersRecursively(folderPath, subfolders);

            return subfolders;
        }

        private static void GetFoldersRecursively(string folderPath, List<string> folders)
        {
            string[] subfolders;
            try
            {
                subfolders = Directory.GetDirectories(folderPath);
            }
            catch
            {
                return;
                // Do nothing intentionally to gather as many folders as possible.
                // Exception may be thrown if the program is not run as administrator,
                //and the caller doesn't have enough rights for a specific folder.
                //That's why we can not simply use Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);
            }

            folders.AddRange(subfolders);

            foreach (string subfolder in subfolders)
            {
                GetFoldersRecursively(subfolder, folders);
            }
        }

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
                    System.Threading.Thread.Sleep(1000);
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
    }
}
