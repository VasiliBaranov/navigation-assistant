using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface IFileSystemListener : IDisposable
    {
        event EventHandler<FileSystemChangeEventArgs> FolderSystemChanged;

        void StartListening(List<string> foldersToListen);

        void StopListening();
    }
}
