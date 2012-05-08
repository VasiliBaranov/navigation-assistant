using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace NavigationAssistant.Core.Utilities
{
    /// <summary>
    /// Implements utility methods for dealing with directories.
    /// </summary>
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

            try
            {
                path = Path.GetFullPath(path);
            }
            catch (PathTooLongException)
            {
                return new List<string>();
            }

            List<string> folders = path
                .Split(new[] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            return folders;
        }

        public static bool IsEmpty(string path)
        {
            DirectoryInfo folder = new DirectoryInfo(path);

            return ListUtility.IsNullOrEmpty(folder.GetFiles()) && ListUtility.IsNullOrEmpty(folder.GetDirectories());
        }

        public static void DeleteUpperFolder(string path, int layersCount)
        {
            for (int i = 0; i < layersCount; i++)
            {
                path = Path.GetDirectoryName(path);
            }
            Directory.Delete(path, true);
        }

        public static string GetFullPathSafe(string path)
        {
            try
            {
                return Path.GetFullPath(path);
            }
            catch (PathTooLongException)
            {
                return null;
            }
        }

        public static bool IsPathShort(string path)
        {
            try
            {
                Path.GetFullPath(path);
            }
            catch (PathTooLongException)
            {
                return false;
            }

            return true;
        }
    }
}
