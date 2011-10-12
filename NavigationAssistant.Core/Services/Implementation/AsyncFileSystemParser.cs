using System;
using System.Collections.Generic;
using System.Threading;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class AsyncFileSystemParser : IAsyncFileSystemParser
    {
        #region Fields

        private readonly IFileSystemParser _fileSystemParser;

        //The object is used by simply handling a reference to it.
        //Better to use Threading.Timer than Timers.Timer, as Timers.Timer callbacks
        //may be executed on the same thread (even if SynchronizationObject is null)-i don't know why).
        //Threading.Timer callbacks are always executed on a different thread
        //(see http://stackoverflow.com/questions/1435876/do-c-timers-elapse-on-a-separate-thread).
        //This is very crucial for unit tests, where the calling thread is always busy with a unit test.
        private Timer _threadingTimer;

        private delegate void ParseFileSystemDelegate();

        private readonly int _delayIntervalInSeconds = 60 * 5;

        private FileSystemCache _fileSystem;

        #endregion

        public event EventHandler<ItemEventArgs<FileSystemCache>> ParsingFinished;

        #region Constructors

        public AsyncFileSystemParser(IFileSystemParser fileSystemParser)
        {
            _fileSystemParser = fileSystemParser;
        }

        public AsyncFileSystemParser(IFileSystemParser fileSystemParser, int delayIntervalInSeconds)
        {
            _fileSystemParser = fileSystemParser;
            _delayIntervalInSeconds = delayIntervalInSeconds;
        }

        #endregion

        #region Public Methods

        public void BeginParsing()
        {
            RegisterCacheUpdate(_delayIntervalInSeconds);
        }

        public void Dispose()
        {
            _fileSystemParser.Dispose();
        }

        #endregion

        #region Non Public Methods

        private void RegisterCacheUpdate(int delayIntervalInSeconds)
        {
            if (delayIntervalInSeconds == 0)
            {
                ParseFileSystemAsynchronously();
                return;
            }

            int delayIntervalInMilliseconds = delayIntervalInSeconds * 1000;
            _threadingTimer = new Timer(HandleDelayFinished, null, delayIntervalInMilliseconds, Timeout.Infinite);
        }

        private void HandleDelayFinished(object state)
        {
            ParseFileSystemAsynchronously();
        }

        private void ParseFileSystemAsynchronously()
        {
            ParseFileSystemDelegate parseFileSystem = ParseFileSystem;
            parseFileSystem.BeginInvoke(HandleSystemParsed, parseFileSystem);
        }

        private void ParseFileSystem()
        {
            //Don't set any restrictions on this parsing, as want to grab the entire system.
            List<FileSystemItem> fileSystemItems = _fileSystemParser.GetSubFolders();
            _fileSystem = new FileSystemCache(fileSystemItems, DateTime.Now);
        }

        private void HandleSystemParsed(IAsyncResult asyncResult)
        {
            ParseFileSystemDelegate parseFileSystem = asyncResult.AsyncState as ParseFileSystemDelegate;

            //You may put additional exception handling here.
            //Should always call EndInvoke (see Richter) to catch errors.
            parseFileSystem.EndInvoke(asyncResult);

            if (ParsingFinished != null)
            {
                ParsingFinished(this, new ItemEventArgs<FileSystemCache>(_fileSystem));
            }
        }

        #endregion
    }
}
