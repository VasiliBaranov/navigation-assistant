using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    /// <summary>
    /// Implements filtering file system items according to the settings constraints.
    /// </summary>
    public class FileSystemFilter : IFileSystemFilter
    {
        #region Fields

        private List<string> _foldersToParseInitial;

        private List<string> _excludeFolderTemplates;

        private List<string> _foldersToParse;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the exclude folder templates; i.e. directories to be ignored while displaying matches,
        /// e.g. "temp","Recycle Bin". Each entry is a case-sensitive regular expression.
        /// </summary>
        /// <value>
        /// The exclude folder templates.
        /// </value>
        public List<string> ExcludeFolderTemplates
        {
            get
            {
                return _excludeFolderTemplates;
            }
            set
            {
                _excludeFolderTemplates = value;
            }
        }

        /// <summary>
        /// Gets or sets the folders to parse (i.e. root folders) when searching for folder matches;
        /// e.g. "C:\Documents and Settings".
        /// </summary>
        /// <value>
        /// The folders to parse.
        /// </value>
        public List<string> FoldersToParse
        {
            get
            {
                return _foldersToParseInitial;
            }
            set
            {
                _foldersToParse = NormalizeFolders(value);
                _foldersToParseInitial = value;
            }
        }

        #endregion

        #region Public Methods

        public bool IsCorrect(FileSystemItem item)
        {
            List<Regex> excludeRegexes = GetExcludeRegexes(_excludeFolderTemplates);
            return IsCorrect(item, _foldersToParse, excludeRegexes);
        }

        public List<FileSystemItem> FilterItems(List<FileSystemItem> items)
        {
            return FilterCache(items, _foldersToParse, _excludeFolderTemplates);
        }

        #endregion

        #region Non Public Methods

        private static List<FileSystemItem> FilterCache(List<FileSystemItem> items,
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

        private static bool IsCorrect(FileSystemItem item, List<string> rootFolders, List<Regex> excludeRegexes)
        {
            return IsInRootFolder(item, rootFolders) && !ShouldBeExcluded(item, excludeRegexes);
        }

        private static List<Regex> GetExcludeRegexes(List<string> excludeFolderTemplates)
        {
            if (ListUtility.IsNullOrEmpty(excludeFolderTemplates))
            {
                return new List<Regex>();
            }

            List<Regex> excludeRegexes = excludeFolderTemplates
                .Select(t=>string.Format(CultureInfo.InvariantCulture, "\\b{0}\\b", t))
                .Select(t => new Regex(t, RegexOptions.IgnoreCase))
                .ToList();
            return excludeRegexes;
        }

        private static bool IsInRootFolder(FileSystemItem item, List<string> rootFolders)
        {
            if (ListUtility.IsNullOrEmpty(rootFolders))
            {
                return true;
            }

            return rootFolders.Any(rf => IsInRootFolder(item.FullPath, rf));
        }

        private static bool IsInRootFolder(string fullPath, string rootFolderPath)
        {
            //root folder has no trailing slashes after NormalizeFolders
            return fullPath.StartsWith(rootFolderPath + "\\", StringComparison.CurrentCulture) ||
                string.Equals(rootFolderPath, fullPath, StringComparison.CurrentCulture);
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

        private static List<string> NormalizeFolders(IEnumerable<string> folders)
        {
            if (folders == null)
            {
                return new List<string>();
            }

            List<string> fullPathFolders = folders
                .Select(s => s.TrimEnd('\\') + "\\") //GetFullPath will return current path for the folder "C:", so we add a backslash manually
                .Select(Path.GetFullPath)
                .Select(s => s.TrimEnd('\\')) //assume in other methods of this class
                .ToList();

            IEnumerable<string> upperCaseFolders = fullPathFolders.Select(StringUtility.MakeFirstLetterUppercase);
            IEnumerable<string> lowerCaseFolders = fullPathFolders.Select(StringUtility.MakeFirstLetterLowercase);

            List<string> normalizedFolders = new List<string>(upperCaseFolders);
            normalizedFolders.AddRange(lowerCaseFolders);

            return normalizedFolders;
        }

        #endregion
    }
}
