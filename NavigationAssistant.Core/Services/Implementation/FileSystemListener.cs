using System;
using System.Collections.Generic;
using System.IO;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class FileSystemListener : IFileSystemListener
    {
        private readonly List<FileSystemWatcher> _fileSystemWatchers;
        private bool _stopped = true;

        public FileSystemListener()
        {
            _fileSystemWatchers = new List<FileSystemWatcher>();
        }

        public event EventHandler<FileSystemChangeEventArgs> FolderSystemChanged;

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
                fileSystemWatcher.Path = path;

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
            }

            _fileSystemWatchers.Clear();
            _stopped = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // get rid of managed resources
                StopListening();
            }
            // get rid of unmanaged resources
        }

        ~FileSystemListener()
        {
            Dispose(false);
        }

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
                FolderSystemChanged(this, new FileSystemChangeEventArgs(oldPath, newPath));
            }
        }
    }
}
