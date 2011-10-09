using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NavigationAssistant.Core.Model;
using System.Linq;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class FileSystemParser : IFileSystemParser
    {
        #region Fields

        private readonly IFileSystemListener _fileSystemListener;

        private List<FileSystemChangeEventArgs> _fileSystemChangeEvents;

        #endregion

        #region Constructors

        public FileSystemParser(IFileSystemListener fileSystemListener)
        {
            _fileSystemListener = fileSystemListener;
        }

        public FileSystemParser(IFileSystemListener fileSystemListener, List<string> foldersToParse)
        {
            _fileSystemListener = fileSystemListener;
            FoldersToParse = foldersToParse;
        }

        #endregion

        #region Properties

        //Note: currently this proeprty is not used in this class
        public List<string> ExcludeFolderTemplates { get; set; }

        public List<string> FoldersToParse { get; set; }

        #endregion

        #region Public Methods

        public List<FileSystemItem> GetSubFolders()
        {
            _fileSystemChangeEvents = new List<FileSystemChangeEventArgs>();

            //Here we register a handler to file system events on a new thread to ensure that they are handled on this new thread,
            //which will not be occupied with GetSubfolderDirectly method.
            //Can not use BeginInvoke/EndInvoke model, as then events sometimes may be put into the main thread queue.
            Thread fileSystemListenerThread = new Thread(RunListener);
            fileSystemListenerThread.Start();

            List<FileSystemItem> subfolders;
            try
            {
                subfolders = GetSubfolderDirectly(FoldersToParse);
                subfolders = ProcessSubFolders(subfolders);
            }
            finally
            {
                fileSystemListenerThread.Abort();
                _fileSystemListener.StopListening();
            }

            foreach (FileSystemChangeEventArgs fileSystemChangeArg in _fileSystemChangeEvents)
            {
                UpdateFolders(subfolders, fileSystemChangeArg, null);
            }

            return subfolders;
        }

        public static void UpdateFolders(List<FileSystemItem> folders, FileSystemChangeEventArgs e, Predicate<FileSystemItem> isCorrectPredicate)
        {
            if (folders == null || e == null)
            {
                return;
            }

            bool changed = false;

            if (!String.IsNullOrEmpty(e.OldFullPath))
            {
                FileSystemItem deletedItem = FindItem(folders, e.OldFullPath);
                if (deletedItem != null)
                {
                    folders.Remove(deletedItem);
                }

                changed = true;
            }

            if (!String.IsNullOrEmpty(e.NewFullPath))
            {
                FileSystemItem addedItem = new FileSystemItem(e.NewFullPath);

                //Duplicates may appear in certain cases. Example:
                //We store all the file system events gathered while parsing the system in a special list,
                //and try to update the list for each of the events after parsing.
                //But if an added folder was parsed after the event for this folder had occurred,
                //the duplicate will be present.
                FileSystemItem duplicate = FindItem(folders, e.NewFullPath);

                bool isCorrect = (isCorrectPredicate == null) || isCorrectPredicate(addedItem);
                if (isCorrect && duplicate == null)
                {
                    folders.Add(addedItem);
                }

                changed = true;
            }

            if (changed)
            {
                folders.Sort(CompareFileSystemItems);
            }
        }

        public void Dispose()
        {
            _fileSystemListener.Dispose();
        }

        #endregion

        #region Non Public Methods

        private static int CompareFileSystemItems(FileSystemItem x, FileSystemItem y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return string.CompareOrdinal(x.FullPath, y.FullPath);
        }

        private void RunListener()
        {
            _fileSystemListener.FolderSystemChanged += HandleFolderSystemChanged;
            _fileSystemListener.StartListening(FoldersToParse);
        }

        //Introduced for testing purposes only.
        protected virtual List<FileSystemItem> ProcessSubFolders(List<FileSystemItem> subfolders)
        {
            return subfolders;
        }

        //All these events will be called after the full system parsing is finished
        private void HandleFolderSystemChanged(object sender, FileSystemChangeEventArgs e)
        {
            _fileSystemChangeEvents.Add(e);
        }

        private static List<FileSystemItem> GetSubfolderDirectly(List<string> rootFolders)
        {
            if (ListUtility.IsNullOrEmpty(rootFolders))
            {
                rootFolders = DirectoryUtility.GetHardDriveRootFolders();
            }

            List<FileSystemItem> subfolders = new List<FileSystemItem>();

            foreach (string rootFolder in rootFolders)
            {
                DirectoryInfo rootFolderInfo;
                try
                {
                    rootFolderInfo = new DirectoryInfo(rootFolder);
                }
                catch
                {
                    //Not enough permissions.
                    continue;
                }

                subfolders.AddRange(GetFoldersRecursively(rootFolderInfo));
                subfolders.Add(GetFileSystemItem(rootFolderInfo));
            }

            return subfolders;
        }

        private static FileSystemItem GetFileSystemItem(DirectoryInfo directoryInfo)
        {
            return new FileSystemItem(directoryInfo.FullName);
        }

        private static FileSystemItem FindItem(List<FileSystemItem> items, string fullPath)
        {
            FileSystemItem item = items.FirstOrDefault(i => String.Equals(i.FullPath, fullPath, StringComparison.Ordinal));
            return item;
        }

        //BTW, using DirectoryInfo.GetDirectories is almost two times faster than Directory.GetDirectories.
        private static List<FileSystemItem> GetFoldersRecursively(DirectoryInfo folder)
        {
            List<FileSystemItem> subfolders = new List<FileSystemItem>();
            GetFoldersRecursively(folder, subfolders);

            return subfolders;
        }

        private static void GetFoldersRecursively(DirectoryInfo folder, List<FileSystemItem> folders)
        {
            DirectoryInfo[] subfolders;
            try
            {
                subfolders = folder.GetDirectories();
            }
            catch
            {
                return;
                // Do nothing intentionally to gather as many folders as possible.
                // Exception may be thrown if the program is not run as administrator,
                //and the caller doesn't have enough rights for a specific folder.
                //That's why we can not simply use Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);
            }

            IEnumerable<FileSystemItem> subfolderItems = subfolders.Select(GetFileSystemItem);
            folders.AddRange(subfolderItems);

            foreach (DirectoryInfo subfolder in subfolders)
            {
                GetFoldersRecursively(subfolder, folders);
            }
        }

        #endregion
    }
}
