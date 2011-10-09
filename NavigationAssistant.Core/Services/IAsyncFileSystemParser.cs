using System;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services
{
    public interface IAsyncFileSystemParser : IDisposable
    {
        event EventHandler<ItemEventArgs<FileSystemCache>> ParsingFinished;
        void BeginParsing();
    }
}
