﻿using System;
using System.Collections.Generic;
using System.Timers;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class AsyncFileSystemParser : IAsyncFileSystemParser
    {
        #region Fields

        private readonly IFileSystemParser _fileSystemParser;

        private readonly Timer _delayTimer;

        private delegate void ParseFileSystemDelegate();

        private readonly int _delayIntervalInSeconds = 60 * 5;

        private FileSystemCache _fileSystem;

        #endregion

        public event EventHandler<ItemEventArgs<FileSystemCache>> ParsingFinished;

        #region Constructors

        public AsyncFileSystemParser(IFileSystemParser fileSystemParser)
        {
            _fileSystemParser = fileSystemParser;
            _delayTimer = new Timer();
        }

        public AsyncFileSystemParser(IFileSystemParser fileSystemParser, int delayIntervalInSeconds)
        {
            _fileSystemParser = fileSystemParser;
            _delayTimer = new Timer();
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

            _delayTimer.Interval = delayIntervalInMilliseconds;
            _delayTimer.Elapsed += HandleDelayFinished;

            //should raise the System.Timers.Timer.Elapsed event only once
            _delayTimer.AutoReset = false;
        }

        private void HandleDelayFinished(object sender, ElapsedEventArgs e)
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