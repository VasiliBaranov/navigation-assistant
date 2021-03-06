﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NavigationAssistant.Core.Model;
using System.Linq;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    /// <summary>
    /// Implements basic parsing of file system folders.
    /// </summary>
    public class FileSystemParser : IFileSystemParser
    {
        #region Fields

        private readonly IFileSystemListener _fileSystemListener;

        private List<FileSystemChangeEventArgs> _fileSystemChangeEvents;

        private bool _listenerLaunched;

        private readonly object _listenerLaunchSync = new object();

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

        /// <summary>
        /// Gets or sets the exclude folder templates; i.e. directories to be ignored while displaying matches,
        /// e.g. "temp","Recycle Bin". Each entry is a case-sensitive regular expression.
        /// </summary>
        /// <value>
        /// The exclude folder templates.
        /// </value>
        /// <remarks>
        /// Note: currently this proeprty is not used in this class
        /// </remarks>
        public List<string> ExcludeFolderTemplates { get; set; }

        /// <summary>
        /// Gets or sets the folders to parse (i.e. root folders) when searching for folder matches;
        /// e.g. "C:\Documents and Settings".
        /// </summary>
        /// <value>
        /// The folders to parse.
        /// </value>
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
                AbortListenerThread(fileSystemListenerThread);
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

            if (!string.IsNullOrEmpty(e.OldFullPath))
            {
                FileSystemItem deletedItem = FindItem(folders, e.OldFullPath);
                if (deletedItem != null)
                {
                    folders.Remove(deletedItem);
                }
            }

            if (string.IsNullOrEmpty(e.NewFullPath))
            {
                return;
            }

            FileSystemItem addedItem;
            try
            {
                addedItem = new FileSystemItem(e.NewFullPath);
            }
            catch (PathTooLongException)
            {
                return;
            }

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
        }

        public void Dispose()
        {
            _fileSystemListener.Dispose();
        }

        #endregion

        #region Non Public Methods

        private void AbortListenerThread(Thread thread)
        {
            while (true)
            {
                bool listenerLaunched;
                lock (_listenerLaunchSync)
                {
                    listenerLaunched = _listenerLaunched;
                }

                //Should ALWAYS check whether the code has executed, because (if parsing is very fast)
                //we may try to abort the thread in the middle of FileSystemWatcher execution,
                //and it may lead to inconsistent folder permissions/state/writes (especially in unit tests).
                if (listenerLaunched)
                {
                    _fileSystemListener.StopListening();
                    thread.Abort();
                    return;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }

        private void RunListener()
        {
            _fileSystemListener.FolderSystemChanged += HandleFolderSystemChanged;
            _fileSystemListener.StartListening(FoldersToParse);

            lock (_listenerLaunchSync)
            {
                _listenerLaunched = true;
            }
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

                FileSystemItem rootItem = GetFileSystemItem(rootFolderInfo);
                if (rootItem != null)
                {
                    subfolders.Add(rootItem);
                }
            }

            return subfolders;
        }

        private static FileSystemItem GetFileSystemItem(DirectoryInfo directoryInfo)
        {
            try
            {
                return new FileSystemItem(directoryInfo.FullName);
            }
            catch (PathTooLongException)
            {
                return null;
            }
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

            IEnumerable<FileSystemItem> subfolderItems = subfolders.Select(GetFileSystemItem).Where(i => i != null);
            folders.AddRange(subfolderItems);

            foreach (DirectoryInfo subfolder in subfolders)
            {
                GetFoldersRecursively(subfolder, folders);
            }
        }

        #endregion
    }
}
