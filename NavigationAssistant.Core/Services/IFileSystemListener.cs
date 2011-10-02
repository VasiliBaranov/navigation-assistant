using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface IFileSystemListener
    {
        event EventHandler<FileSystemChangeEventArgs> FileSystemChanged;

        void StartListening(List<string> foldersToListen);

        void StopListening();
    }
}
