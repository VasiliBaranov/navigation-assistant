using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    //Use this class with Singleton lifetime from IoC
    public class CachedFileSystemParser : IFileSystemParser
    {
        #region Fields

        private readonly IFileSystemParser _fileSystemParser;

        private readonly ICacheSerializer _cacheSerializer;

        private readonly IFileSystemListener _fileSystemListener;

        private List<FileSystemItem> _cache;

        private List<FileSystemItem> _fullCache; 

        private readonly Timer _delayTimer;

        private delegate void UpdateCacheDelegate();

        private readonly object _fullCacheSync = new object();

        private bool _includeHiddenFolders;

        private List<string> _excludeFolderTemplates;

        private List<string> _foldersToParse;

        private const int UpdatesCountToWrite = 500;

        private int _updatesCount;

        private int _delayIntervalInSeconds;

        #endregion

        #region Properties

        public bool IncludeHiddenFolders
        {
            get { return _includeHiddenFolders; }
            set
            {
                _includeHiddenFolders = value;
                ResetCache();
            }
        }

        public List<string> ExcludeFolderTemplates
        {
            get { return _excludeFolderTemplates; }
            set
            {
                _excludeFolderTemplates = value;
                ResetCache();
            }
        }

        public List<string> FoldersToParse
        {
            get { return _foldersToParse; }
            set
            {
                _foldersToParse = value;
                ResetCache();
            }
        }

        public IFileSystemParser FileSystemParser
        {
            get { return _fileSystemParser; }
        }

        public ICacheSerializer CacheSerializer
        {
            get { return _cacheSerializer; }
        }

        public IFileSystemListener FileSystemListener
        {
            get { return _fileSystemListener; }
        }

        public int DelayIntervalInSeconds
        {
            get { return _delayIntervalInSeconds; }
            set { _delayIntervalInSeconds = value; }
        }

        #endregion

        #region Constructors

        public CachedFileSystemParser(IFileSystemParser fileSystemParser,
            ICacheSerializer cacheSerializer,
            IFileSystemListener fileSystemListener,
            int delayIntervalInSeconds)
        {
            _fileSystemParser = fileSystemParser;
            _cacheSerializer = cacheSerializer;
            _fileSystemListener = fileSystemListener;
            _delayIntervalInSeconds = delayIntervalInSeconds;
            _delayTimer = new Timer();

            bool fullCacheUpToDate = ReadFullCache();

            // Listen to the changes in the whole system to update the fullCache.
            _fileSystemListener.FolderSystemChanged += HandleFolderSystemChanged;
            _fileSystemListener.StartListening(null);

            //Set up a timer for initial cache update
            if (!fullCacheUpToDate)
            {
                RegisterCacheUpdate(_delayIntervalInSeconds);
            }
        }

        #endregion

        #region Public Methods

        public List<FileSystemItem> GetSubFolders()
        {
            //Use synchronization to avoid conflicts with the cache update after expiration.
            lock (_fullCacheSync)
            {
                if (_cache == null)
                {
                    _cache = FilterCache(_fullCache, _foldersToParse, _excludeFolderTemplates);
                }

                return _cache;
            }
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
                _fileSystemParser.Dispose();
                _fileSystemListener.Dispose();
                _delayTimer.Dispose();
            }

            // get rid of unmanaged resources
            try
            {
                lock (_fullCacheSync)
                {
                    _cacheSerializer.SerializeCache(_fullCache);
                }
            }
            catch
            {
                // no exceptions in finalizer
            }
        }

        ~CachedFileSystemParser()
        {
            Dispose(false);
        }

        #endregion

        #region Non Public Methods

        private void RegisterCacheUpdate(int delayIntervalInSeconds)
        {
            _delayTimer.Interval = delayIntervalInSeconds * 1000;
            _delayTimer.Elapsed += HandleDelayFinished;

            //should raise the System.Timers.Timer.Elapsed event only once
            _delayTimer.AutoReset = false;
        }

        private bool ReadFullCache()
        {
            bool fullCacheUpToDate = false;

            //Parse file system.
            //We always keep the full cache in memory for the following reasons:
            //1. not to re-read it
            //2. the cache file may be deleted while this class is operating
            _fullCache = _cacheSerializer.DeserializeCache();
            if (_fullCache == null)
            {
                //The application is loaded for the first time (no cache stored on disk).
                //Run this method in the main thread, thus freezing it.
                UpdateCacheUnsynchronized();
                fullCacheUpToDate = true;
            }

            return fullCacheUpToDate;
        }

        private void HandleFolderSystemChanged(object sender, FileSystemChangeEventArgs e)
        {
            lock (_fullCacheSync)
            {
                Implementation.FileSystemParser.UpdateFolders(_fullCache, e, null);
                _fullCache = _fullCache.OrderBy(item => item.FullPath).ToList();

                _updatesCount++;

                if (_updatesCount > UpdatesCountToWrite)
                {
                    _cacheSerializer.SerializeCache(_fullCache);
                    _updatesCount = 0;
                }

                if (_cache != null)
                {
                    Implementation.FileSystemParser.UpdateFolders(_cache, e, IsCorrect);
                }
            }
        }

        private void ResetCache()
        {
            _cache = null;
        }

        private static List<FileSystemItem> FilterCache(List<FileSystemItem> items,
            List<string> rootFolders, List<string> excludeFolderTemplates)
        {
            List<Regex> excludeRegexes = GetExcludeRegexes(excludeFolderTemplates);

            List<FileSystemItem> filteredItems = items
                .Where(item => IsCorrect(item, rootFolders, excludeRegexes))
                .ToList();

            return filteredItems;
        }

        private bool IsCorrect(FileSystemItem item)
        {
            List<Regex> excludeRegexes = GetExcludeRegexes(_excludeFolderTemplates);
            return IsCorrect(item, _foldersToParse, excludeRegexes);
        }

        private static bool IsCorrect(FileSystemItem item, List<string> rootFolders, List<Regex> excludeRegexes)
        {
            return IsInRootFolder(item, rootFolders) && !ShouldBeExcluded(item, excludeRegexes);
        }

        private static List<Regex> GetExcludeRegexes(List<string> excludeFolderTemplates)
        {
            List<Regex> excludeRegexes = excludeFolderTemplates.Select(t => new Regex(t, RegexOptions.IgnoreCase)).ToList();
            return excludeRegexes;
        }

        private static bool IsInRootFolder(FileSystemItem item, List<string> rootFolders)
        {
            if (ListUtility.IsNullOrEmpty(rootFolders))
            {
                return true;
            }

            return rootFolders.Any(rootFolder => item.FullPath.StartsWith(rootFolder));
        }

        private static bool ShouldBeExcluded(FileSystemItem item, List<Regex> excludeRegexes)
        {
            List<string> foldersInPath = DirectoryUtility.SplitPath(item.FullPath);

            foreach (string folder in foldersInPath)
            {
                foreach (Regex excludeRegex in excludeRegexes)
                {
                    if (excludeRegex.IsMatch(folder))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void UpdateCache()
        {
            lock (_fullCacheSync)
            {
                UpdateCacheUnsynchronized();
            }
        }

        private void UpdateCacheUnsynchronized()
        {
            //Don't set any restrictions on this parsing, as want to grab the entire system.
            _fullCache = _fileSystemParser.GetSubFolders();
            _cacheSerializer.SerializeCache(_fullCache);
        }

        private static void HandleCacheUpdated(IAsyncResult asyncResult)
        {
            UpdateCacheDelegate updateCache = asyncResult.AsyncState as UpdateCacheDelegate;

            //You may put additional exception handling here.
            //Should always call EndInvoke (see Richter) to catch errors.
            updateCache.EndInvoke(asyncResult);
        }

        private void UpdateCacheAsynchronously()
        {
            UpdateCacheDelegate updateCache = UpdateCache;
            updateCache.BeginInvoke(HandleCacheUpdated, updateCache);
        }

        private void HandleDelayFinished(object sender, ElapsedEventArgs e)
        {
            UpdateCacheAsynchronously();
        }

        #endregion
    }
}