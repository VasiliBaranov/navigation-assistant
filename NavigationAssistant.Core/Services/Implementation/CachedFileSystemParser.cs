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

        private List<FileSystemItem> _cache;

        private readonly Timer _expirationTimer;

        private delegate void UpdateCacheDelegate();

        private readonly object _cacheSync = new object();

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

        public CachedFileSystemParser(IFileSystemParser fileSystemParser, ICacheSerializer cacheSerializer, int expirationIntervalInSeconds)
        {
            _fileSystemParser = fileSystemParser;
            _cacheSerializer = cacheSerializer;

            _expirationTimer = new Timer();
            _expirationTimer.Interval = expirationIntervalInSeconds * 1000;
            _expirationTimer.Elapsed += HandleCacheExpired;

            //should raise the System.Timers.Timer.Elapsed event only once
            _expirationTimer.AutoReset = false;
        }

        #endregion

        #region Public Methods

        public List<FileSystemItem> GetSubFolders()
        {
            //Use synchronization to avoid conflicts with the cache update after expiration.
            lock (_cacheSync)
            {
                if (_cache == null)
                {
                    //Method is called for the first time
                    _cache = _cacheSerializer.DeserializeCache();
                    if (_cache == null)
                    {
                        //The application is loaded for the first time (no cache stored on disk).
                        //Run this method in the main thread, thus freezing it.
                        UpdateCacheUnsynchronized();
                    }
                    else
                    {
                        _cache = FilterCache(_cache, _foldersToParse, _excludeFolderTemplates, _includeHiddenFolders);
                        _expirationTimer.Start();
                    }
                }

                return _cache;
            }
        }

        #endregion

        #region Non Public Methods

        private void ResetCache()
        {
            lock (_cacheSync)
            {
                _cache = null;
            }
        }

        private static List<FileSystemItem> FilterCache(List<FileSystemItem> items,
            List<string> rootFolders, List<string> excludeFolderTemplates, bool includeHiddenFolders)
        {
            List<Regex> excludeRegexes = excludeFolderTemplates.Select(t => new Regex(t, RegexOptions.IgnoreCase)).ToList();

            List<FileSystemItem> filteredItems = items
                .Where(item => IsInRootFolder(item, rootFolders) && !ShouldBeExcluded(item, excludeRegexes))
                .ToList();

            return filteredItems;
        }

        private static bool IsInRootFolder(FileSystemItem item, List<string> rootFolders)
        {
            if (ListUtility.IsNullOrEmpty(rootFolders))
            {
                return true;
            }

            return rootFolders.Any(rootFolder => item.ItemPath.StartsWith(rootFolder));
        }

        private static bool ShouldBeExcluded(FileSystemItem item, List<Regex> excludeRegexes)
        {
            List<string> foldersInPath = DirectoryUtility.SplitPath(item.ItemPath);

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
            lock (_cacheSync)
            {
                UpdateCacheUnsynchronized();
            }
        }

        private void UpdateCacheUnsynchronized()
        {
            //Don't set any restrictions on this parsing, as want to grab the entire system.
            List<FileSystemItem> folders = _fileSystemParser.GetSubFolders();
            _cacheSerializer.SerializeCache(folders);

            _cache = FilterCache(folders, _foldersToParse, _excludeFolderTemplates, _includeHiddenFolders);

            _expirationTimer.Start();
        }

        private void HandleCacheUpdated(IAsyncResult asyncResult)
        {
            UpdateCacheDelegate updateCache = asyncResult.AsyncState as UpdateCacheDelegate;

            //You may put additional exception handling here.
            //Should always call EndInvoke (see Richter).
            updateCache.EndInvoke(asyncResult);
        }

        private void UpdateCacheAsynchronously()
        {
            UpdateCacheDelegate updateCache = UpdateCache;
            updateCache.BeginInvoke(HandleCacheUpdated, updateCache);
        }

        private void HandleCacheExpired(object sender, ElapsedEventArgs e)
        {
            UpdateCacheAsynchronously();
        }

        #endregion
    }
}