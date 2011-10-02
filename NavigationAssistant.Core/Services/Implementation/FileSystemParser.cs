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

        #endregion

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
            bool handled = HandleAttributesChange(folders, e);
            if (handled)
            {
                return;
            }

            if (!string.IsNullOrEmpty(e.OldFullPath))
            {
                FileSystemItem deletedItem = DirectoryUtility.FindItem(folders, e.OldFullPath);
                if (deletedItem != null)
                {
                    folders.Remove(deletedItem);
                }
            }

            if (!string.IsNullOrEmpty(e.NewFullPath))
            {
                FileSystemItem addedItem = new FileSystemItem(e.NewFullPath);

                //Duplicates may appear in certain cases. Example:
                //We store all the file system events gathered while parsing the system in a special list,
                //and try to update the list for each of the events after parsing.
                //But if an added folder was parsed after the event for this folder had occurred,
                //the duplicate will be present.
                FileSystemItem duplicate = DirectoryUtility.FindItem(folders, e.NewFullPath);

                bool isCorrect = (isCorrectPredicate == null) || isCorrectPredicate(addedItem);
                if (isCorrect && duplicate == null)
                {
                    folders.Add(addedItem);
                }
            }
        }

        #endregion

        #region Non Public Methods

        private static bool HandleAttributesChange(List<FileSystemItem> folders, FileSystemChangeEventArgs e)
        {
            bool propertiesChanged =
                !string.IsNullOrEmpty(e.OldFullPath) &&
                !string.IsNullOrEmpty(e.NewFullPath) &&
                string.Equals(e.OldFullPath, e.NewFullPath, StringComparison.Ordinal);
            if (!propertiesChanged)
            {
                return false;
            }

            FileSystemItem changedItem = DirectoryUtility.FindItem(folders, e.OldFullPath);
            if (changedItem != null)
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(e.NewFullPath);
                    changedItem.IsHidden = directoryInfo.IsHidden();
                }
                catch (Exception)
                {
                    //Not enough rights
                }
            }
            return true;
        }

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
                subfolders.Add(DirectoryUtility.GetFileSystemItem(rootFolderInfo));
            }

            return subfolders;
        }

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

            IEnumerable<FileSystemItem> subfolderItems = subfolders.Select(DirectoryUtility.GetFileSystemItem);
            folders.AddRange(subfolderItems);

            foreach (DirectoryInfo subfolder in subfolders)
            {
                GetFoldersRecursively(subfolder, folders);
            }
        }

        #endregion
    }
}
