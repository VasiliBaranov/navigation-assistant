using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Core.Model;

namespace Core.Services.Implementation
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

        private const int ExpirationIntervalInSeconds = 10 * 60; //10 minutes

        #endregion

        #region Constructors

        public CachedFileSystemParser(IFileSystemParser fileSystemParser, ICacheSerializer cacheSerializer)
        {
            _fileSystemParser = fileSystemParser;
            _cacheSerializer = cacheSerializer;

            _expirationTimer = new Timer();
            _expirationTimer.Interval = ExpirationIntervalInSeconds * 1000;
            _expirationTimer.Elapsed += HandleCacheExpired;

            //should raise the System.Timers.Timer.Elapsed event only once
            _expirationTimer.AutoReset = false;
        }

        #endregion

        #region Public Methods

        public List<FileSystemItem> GetFolders(List<string> rootFolders)
        {
            if (Utilities.IsNullOrEmpty(rootFolders))
            {
                throw new ArgumentNullException("rootFolders");
            }

            //Use synchronization to avoid conflicts with the cache update after expiration.
            lock (_cacheSync)
            {
                if (_cache != null)
                {
                    return FilterCacheItems(_cache, rootFolders);
                }

                //Method is called for the first time
                _cache = _cacheSerializer.DeserializeCache();
                if (_cache != null)
                {
                    _expirationTimer.Start();
                    return FilterCacheItems(_cache, rootFolders);
                }
            }

            //The application is loaded for the first time (no cache stored on disk).
            //Run this method in the main thread, thus freezing it.
            UpdateCache();

            lock (_cacheSync)
            {
                return FilterCacheItems(_cache, rootFolders);
            }
        }

        private List<FileSystemItem> FilterCacheItems(List<FileSystemItem> items, List<string> rootFolders)
        {
            List<FileSystemItem> filteredItems = new List<FileSystemItem>();
            foreach (FileSystemItem item in items)
            {
                bool isInRootFolder = rootFolders.Any(rootFolder => item.ItemPath.StartsWith(rootFolder));

                if (isInRootFolder)
                {
                    filteredItems.Add(item);
                }
            }

            return filteredItems;
        }

        private void UpdateCache()
        {
            List<string> rootFolders = Utilities.GetHardDriveRootFolders();
            List<FileSystemItem> folders = _fileSystemParser.GetFolders(rootFolders);

            lock (_cacheSync)
            {
                _cache = folders;
                _cacheSerializer.SerializeCache(_cache);

                _expirationTimer.Start();
            }
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