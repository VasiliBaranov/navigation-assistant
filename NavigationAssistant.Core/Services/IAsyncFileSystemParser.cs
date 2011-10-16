using System;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines methods and events for asynchronous file system parsing.
    /// </summary>
    public interface IAsyncFileSystemParser : IDisposable
    {
        event EventHandler<ItemEventArgs<FileSystemCache>> ParsingFinished;

        void BeginParsing();
    }
}
