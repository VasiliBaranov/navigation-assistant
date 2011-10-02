using System;
using System.Collections.Generic;
using System.Threading;
using NavigationAssistant.Core.Model;
using System.Linq;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class FileSystemParser : IFileSystemParser
    {
        private readonly IFileSystemListener _fileSystemListener;

        private List<FileSystemItem> _subfolders;

        private List<FileSystemChangeEventArgs> _fileSystemChangeEvents;

        public FileSystemParser(IFileSystemListener fileSystemListener)
        {
            _fileSystemListener = fileSystemListener;
        }

        //Note: currently these proeprties are not used in this class, as it is always called with nulls.
        #region Properties

        public bool IncludeHiddenFolders { get; set; }

        public List<string> ExcludeFolderTemplates { get; set; }

        public List<string> FoldersToParse { get; set; }

        #endregion

        #region Public Methods

        public List<FileSystemItem> GetSubFolders()
        {
            Thread fileSystemListenerThread = new Thread(RunListener);
            fileSystemListenerThread.Start();

            try
            {
                _subfolders = GetSubfolderDirectly(FoldersToParse);
                _subfolders = ProcessSubFolders(_subfolders);
            }
            finally
            {
                fileSystemListenerThread.Abort();
                _fileSystemListener.StopListening();
            }

            foreach (FileSystemChangeEventArgs fileSystemChangeEvent in _fileSystemChangeEvents)
            {
                UpdateFolders(_subfolders, fileSystemChangeEvent, null);
            }

            return _subfolders;
        }

        public static void UpdateFolders(List<FileSystemItem> folders, FileSystemChangeEventArgs e, Predicate<FileSystemItem> isCorrectPredicate)
        {
            if (!String.IsNullOrEmpty(e.OldPath))
            {
                FileSystemItem deletedItem = FindItem(folders, e.OldPath);
                if (deletedItem != null)
                {
                    folders.Remove(deletedItem);
                }
            }

            if (!String.IsNullOrEmpty(e.NewPath))
            {
                FileSystemItem addedItem = new FileSystemItem(e.NewPath);

                //Duplicates may appear in certain cases. Example:
                //We store all the file system events gathered while parsing the system in a special list,
                //and try to update the list for each of the events after parsing.
                //But if an added folder was parsed after the event for this folder had occurred,
                //the duplicate will be present.
                FileSystemItem duplicate = FindItem(folders, e.NewPath);

                bool isCorrect = (isCorrectPredicate == null) || isCorrectPredicate(addedItem);
                if (isCorrect && duplicate == null)
                {
                    folders.Add(addedItem);
                }
            }
        }

        #endregion

        #region Non Public Methods

        private void RunListener()
        {
            _fileSystemListener.FileSystemChanged += HandleFileSystemChanged;

            _fileSystemChangeEvents = new List<FileSystemChangeEventArgs>();

            _fileSystemListener.StartListening(FoldersToParse);
        }

        protected virtual List<FileSystemItem> ProcessSubFolders(List<FileSystemItem> subfolders)
        {
            return subfolders;
        }

        //All these events will be called after the full system parsing is finished
        private void HandleFileSystemChanged(object sender, FileSystemChangeEventArgs e)
        {
            _fileSystemChangeEvents.Add(e);
        }

        private static FileSystemItem GetFileSystemInfo(string path)
        {
            return new FileSystemItem(path);
        }

        private static List<FileSystemItem> GetSubfolderDirectly(List<string> rootFolders)
        {
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

        private static FileSystemItem FindItem(List<FileSystemItem> cache, string fullPath)
        {
            FileSystemItem item = cache.FirstOrDefault(i => String.Equals(i.FullPath, fullPath, StringComparison.Ordinal));
            return item;
        }

        #endregion
    }
}
