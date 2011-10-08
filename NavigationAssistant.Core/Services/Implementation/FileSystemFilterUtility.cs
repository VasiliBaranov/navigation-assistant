using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    //Didn't move to utilities as it belongs to CachedFileSystemParser
    public static class FileSystemFilterUtility
    {
        public static List<FileSystemItem> FilterCache(List<FileSystemItem> items,
                                                        List<string> rootFolders, List<string> excludeFolderTemplates)
        {
            if (ListUtility.IsNullOrEmpty(items))
            {
                return new List<FileSystemItem>();
            }

            List<Regex> excludeRegexes = GetExcludeRegexes(excludeFolderTemplates);

            List<FileSystemItem> filteredItems = items
                .Where(item => IsCorrect(item, rootFolders, excludeRegexes))
                .ToList();

            return filteredItems;
        }

        public static bool IsCorrect(FileSystemItem item, List<string> rootFolders, List<Regex> excludeRegexes)
        {
            return IsInRootFolder(item, rootFolders) && !ShouldBeExcluded(item, excludeRegexes);
        }

        public static List<Regex> GetExcludeRegexes(List<string> excludeFolderTemplates)
        {
            if (ListUtility.IsNullOrEmpty(excludeFolderTemplates))
            {
                return new List<Regex>();
            }

            List<Regex> excludeRegexes = excludeFolderTemplates.Select(t => new Regex(t, RegexOptions.IgnoreCase)).ToList();
            return excludeRegexes;
        }

        private static bool IsInRootFolder(FileSystemItem item, List<string> rootFolders)
        {
            if (ListUtility.IsNullOrEmpty(rootFolders))
            {
                return true;
            }

            return rootFolders.Any(rootFolder => item.FullPath.StartsWith(rootFolder));
        }

        private static bool ShouldBeExcluded(FileSystemItem item, List<Regex> excludeRegexes)
        {
            if (ListUtility.IsNullOrEmpty(excludeRegexes))
            {
                return false;
            }

            List<string> foldersInPath = DirectoryUtility.SplitPath(item.FullPath);

            foreach (string folder in foldersInPath)
            {
                foreach (Regex excludeRegex in excludeRegexes)
                {
                    if (excludeRegex.IsMatch(folder))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
