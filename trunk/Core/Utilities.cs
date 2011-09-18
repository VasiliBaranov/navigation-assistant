using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core
{
    public static class Utilities
    {
        public static List<string> SplitStringByUpperChars(string input)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(input))
            {
                return result;
            }

            int previousUpperCharIndex = 0;
            int inputLength = input.Length;

            for (int i = 0; i < inputLength; i++)
            {
                char currentChar = input[i];

                bool shouldAddSubstring = char.IsUpper(currentChar) && (i != 0);

                if (!shouldAddSubstring)
                {
                    continue;
                }

                //E.g. input = myInput; previousUpperCharIndex = 0, i = 2.
                string substring = input.Substring(previousUpperCharIndex, i - previousUpperCharIndex);
                result.Add(substring);

                previousUpperCharIndex = i;
            }

            //Add the last substring
            string lastSubstring = input.Substring(previousUpperCharIndex);
            result.Add(lastSubstring);

            return result;
        }

        public static bool IsNullOrEmpty<T>(ICollection<T> list)
        {
            return list == null || list.Count == 0;
        }

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
    }
}
