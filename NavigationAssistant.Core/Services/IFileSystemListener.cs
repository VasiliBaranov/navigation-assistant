using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines methods and events for listening for file system changes (currently just for folders).
    /// </summary>
    public interface IFileSystemListener : IDisposable
    {
        event EventHandler<FileSystemChangeEventArgs> FolderSystemChanged;

        void StartListening(List<string> foldersToListen);

        void StopListening();
    }
}
