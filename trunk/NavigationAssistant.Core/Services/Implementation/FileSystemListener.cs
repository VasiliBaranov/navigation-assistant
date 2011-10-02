﻿using System;
using System.Collections.Generic;
using System.IO;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class FileSystemListener : IFileSystemListener
    {
        private readonly List<FileSystemWatcher> _fileSystemWatchers;

        public FileSystemListener()
        {
            _fileSystemWatchers = new List<FileSystemWatcher>();
        }

        public event EventHandler<FileSystemChangeEventArgs> FileSystemChanged;

        public void StartListening(List<string> foldersToListen)
        {
            if (ListUtility.IsNullOrEmpty(foldersToListen))
            {
                foldersToListen = DirectoryUtility.GetHardDriveRootFolders();
            }

            foreach (string path in foldersToListen)
            {
                FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
                fileSystemWatcher.Path = path;
                fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName;
                fileSystemWatcher.IncludeSubdirectories = true;

                // Add event handlers.
                fileSystemWatcher.Created += HandleFolderCreated;
                fileSystemWatcher.Deleted += HandleFolderDeleted;
                fileSystemWatcher.Renamed += HandleFolderRenamed;

                _fileSystemWatchers.Add(fileSystemWatcher);

                // Begin watching.
                fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        public void StopListening()
        {
            foreach (FileSystemWatcher fileSystemWatcher in _fileSystemWatchers)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
            }

            _fileSystemWatchers.Clear();
        }

        private void HandleFolderRenamed(object sender, RenamedEventArgs e)
        {
            OnPropertyChanged(e.OldFullPath, e.FullPath);
        }

        private void HandleFolderDeleted(object sender, FileSystemEventArgs e)
        {
            OnPropertyChanged(e.FullPath, null);
        }

        private void HandleFolderCreated(object sender, FileSystemEventArgs e)
        {
            OnPropertyChanged(null, e.FullPath);
        }

        protected virtual void OnPropertyChanged(string oldPath, string newPath)
        {
            if (FileSystemChanged != null)
            {
                FileSystemChanged(this, new FileSystemChangeEventArgs(oldPath, newPath));
            }
        }
    }
}
