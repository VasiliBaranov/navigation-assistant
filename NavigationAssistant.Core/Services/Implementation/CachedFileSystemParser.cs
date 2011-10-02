using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class CachedFileSystemParser : IFileSystemParser
    {
        #region Fields

        private readonly IFileSystemParser _fileSystemParser;

        private readonly ICacheSerializer _cacheSerializer;

        private readonly IFileSystemListener _fileSystemListener;

        private List<FileSystemItem> _cache;

        private List<FileSystemItem> _fullCache; 

        private Timer _delayTimer;

        private delegate void UpdateCacheDelegate();

        private readonly object _fullCacheSync = new object();

        private bool _includeHiddenFolders;

        private List<string> _excludeFolderTemplates;

        private List<string> _foldersToParse;

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

        #endregion

        #region Constructors

        public CachedFileSystemParser(IFileSystemParser fileSystemParser, ICacheSerializer cacheSerializer, IFileSystemListener fileSystemListener,
            int delayIntervalInSeconds)
        {
            _fileSystemParser = fileSystemParser;
            _cacheSerializer = cacheSerializer;
            _fileSystemListener = fileSystemListener;

            bool fullCacheUpToDate = ReadFullCache();

            // Listen to the changes in the whole system to update the fullCache.
            _fileSystemListener.FileSystemChanged += HandleFileSystemChanged;
            _fileSystemListener.StartListening(null); 

            //Set up a timer for initial cache update
            if (!fullCacheUpToDate)
            {
                RegisterCacheUpdate(delayIntervalInSeconds);
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
                    _cache = FilterCache(_fullCache, _foldersToParse, _excludeFolderTemplates, _includeHiddenFolders);
                }

                return _cache;
            }
        }

        #endregion

        #region Non Public Methods

        private void RegisterCacheUpdate(int delayIntervalInSeconds)
        {
            _delayTimer = new Timer();
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

        private void HandleFileSystemChanged(object sender, FileSystemChangeEventArgs e)
        {
            lock (_fullCacheSync)
            {
                FileSystemParser.UpdateFolders(_fullCache, e, null);
                _fullCache = _fullCache.OrderBy(item => item.FullPath).ToList();
                _cacheSerializer.SerializeCache(_fullCache);

                FileSystemParser.UpdateFolders(_cache, e, IsCorrect);
            }
        }

        private void ResetCache()
        {
            _cache = null;
        }

        private static List<FileSystemItem> FilterCache(List<FileSystemItem> items,
            List<string> rootFolders, List<string> excludeFolderTemplates, bool includeHiddenFolders)
        {
            List<Regex> excludeRegexes = GetExcludeRegexes(excludeFolderTemplates);

            List<FileSystemItem> filteredItems = items
                .Where(item => IsInRootFolder(item, rootFolders) && !ShouldBeExcluded(item, excludeRegexes))
                .ToList();

            return filteredItems;
        }

        private bool IsCorrect(FileSystemItem item)
        {
            List<Regex> excludeRegexes = GetExcludeRegexes(_excludeFolderTemplates);
            return IsInRootFolder(item, _foldersToParse) && !ShouldBeExcluded(item, excludeRegexes);
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