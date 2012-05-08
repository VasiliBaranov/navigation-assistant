using System;
using System.Collections.Generic;
using System.IO;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    /// <summary>
    /// Implements listening for file system changes (currently just for folders).
    /// </summary>
    public class FileSystemListener : IFileSystemListener
    {
        #region Fields

        private readonly List<FileSystemWatcher> _fileSystemWatchers;

        private bool _stopped = true;

        public event EventHandler<FileSystemChangeEventArgs> FolderSystemChanged;

        #endregion

        #region Constructors

        public FileSystemListener()
        {
            _fileSystemWatchers = new List<FileSystemWatcher>();
        }

        #endregion

        #region Public Methods

        public void StartListening(List<string> foldersToListen)
        {
            if (!_stopped)
            {
                throw new InvalidOperationException("Call Stop before starting listening.");
            }

            if (ListUtility.IsNullOrEmpty(foldersToListen))
            {
                foldersToListen = DirectoryUtility.GetHardDriveRootFolders();
            }

            foreach (string path in foldersToListen)
            {
                FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(path);
                }
                catch (PathTooLongException)
                {
                    continue;
                }

                fileSystemWatcher.Path = fullPath;

                NotifyFilters filters = NotifyFilters.DirectoryName;

                fileSystemWatcher.NotifyFilter = filters;
                fileSystemWatcher.IncludeSubdirectories = true;

                // Add event handlers.
                fileSystemWatcher.Created += HandleFolderCreated;
                fileSystemWatcher.Deleted += HandleFolderDeleted;
                fileSystemWatcher.Renamed += HandleFolderRenamed;

                _fileSystemWatchers.Add(fileSystemWatcher);

                // Begin watching.
                fileSystemWatcher.EnableRaisingEvents = true;
            }
            _stopped = false;
        }

        public void StopListening()
        {
            foreach (FileSystemWatcher fileSystemWatcher in _fileSystemWatchers)
            {
                fileSystemWatcher.EnableRaisingEvents = false;

                fileSystemWatcher.Created -= HandleFolderCreated;
                fileSystemWatcher.Deleted -= HandleFolderDeleted;
                fileSystemWatcher.Renamed -= HandleFolderRenamed;

                fileSystemWatcher.Dispose();
            }

            _fileSystemWatchers.Clear();
            _stopped = true;
        }

        public void Dispose()
        {
            StopListening();
        }

        #endregion

        #region Non Public Methods

        private void HandleFolderRenamed(object sender, RenamedEventArgs e)
        {
            OnFolderSystemChanged(e.OldFullPath, e.FullPath);
        }

        private void HandleFolderDeleted(object sender, FileSystemEventArgs e)
        {
            OnFolderSystemChanged(e.FullPath, null);
        }

        private void HandleFolderCreated(object sender, FileSystemEventArgs e)
        {
            OnFolderSystemChanged(null, e.FullPath);
        }

        protected virtual void OnFolderSystemChanged(string oldPath, string newPath)
        {
            if (FolderSystemChanged != null)
            {
                try
                {
                    FileSystemChangeEventArgs args = new FileSystemChangeEventArgs(oldPath, newPath);
                    FolderSystemChanged(this, args);
                }
                catch(PathTooLongException)
                {
                    
                }
            }
        }

        #endregion
    }
}
