using System;
using System.Collections.Generic;
using System.IO;
using NavigationAssistant.Core.Model;
using System.Linq;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class FileSystemParser : IFileSystemParser
    {
        #region Properties

        //Note: currently these proeprties are not used in this class.
        //This class is always wrapped by CachedFileSystemParser, which handles these properties.
        //TODO: Implement.
        public bool IncludeHiddenFolders { get; set; }

        public List<string> ExcludeFolderTemplates { get; set; }

        public List<string> FoldersToParse { get; set; }

        #endregion

        #region Public Methods

        public List<FileSystemItem> GetSubFolders()
        {
            List<string> rootFolders = FoldersToParse;
            if (ListUtility.IsNullOrEmpty(rootFolders))
            {
                rootFolders = DirectoryUtility.GetHardDriveRootFolders();
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

        #endregion

        #region Non Public Methods

        private static FileSystemItem GetFileSystemInfo(string path)
        {
            return new FileSystemItem(Path.GetFileName(path), path);
        }

        #endregion
    }
}
