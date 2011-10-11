using System;
using System.Collections.Generic;
using System.IO;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    //Use this class with Singleton lifetime from IoC
    public class CachedFileSystemParser : IFileSystemParser
    {
        #region Services

        private readonly IFileSystemParser _fileSystemParser;

        private readonly ICacheSerializer _cacheSerializer;

        private readonly IFileSystemListener _fileSystemListener;

        private readonly IRegistryService _registryService;

        private readonly IFileSystemFilter _fileSystemFilter;

        private readonly IAsyncFileSystemParser _asyncFileSystemParser;

        #endregion

        #region Supplementary Fields

        private readonly object _cacheSync = new object();

        private readonly int _updatesCountToWrite = 500;

        private const int CacheValidityPeriodInSeconds = 60*2;

        #endregion

        #region Data Fields

        private int _updatesCount;

        //Should be synchronized
        private bool _fullCacheUpToDate;

        //Should be synchronized
        private List<FileSystemItem> _filteredCacheItems;

        //Should be synchronized
        private FileSystemCache _fullCache; 

        #endregion

        #region Properties

        public List<string> ExcludeFolderTemplates
        {
            get
            {
                return _fileSystemFilter.ExcludeFolderTemplates;
            }
            set
            {
                _fileSystemFilter.ExcludeFolderTemplates = value;
                ResetFilteredCacheItems();
            }
        }

        public List<string> FoldersToParse
        {
            get
            {
                return _fileSystemFilter.FoldersToParse;
            }
            set
            {
                _fileSystemFilter.FoldersToParse = value;
                ResetFilteredCacheItems();
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

        public IAsyncFileSystemParser AsyncFileSystemParser
        {
            get { return _asyncFileSystemParser; }
        }

        #endregion

        #region Constructors

        //For test purposes
        public CachedFileSystemParser(IFileSystemParser fileSystemParser,
            ICacheSerializer cacheSerializer,
            IFileSystemListener fileSystemListener,
            IRegistryService registryService,
            IAsyncFileSystemParser asyncFileSystemParser,
            bool appWasAutoRun,
            int updatesCountToWrite)
            : this(fileSystemParser, cacheSerializer, fileSystemListener, registryService, asyncFileSystemParser, appWasAutoRun)
        {
            _updatesCountToWrite = updatesCountToWrite;
        }

        public CachedFileSystemParser(IFileSystemParser fileSystemParser,
            ICacheSerializer cacheSerializer,
            IFileSystemListener fileSystemListener,
            IRegistryService registryService,
            IAsyncFileSystemParser asyncFileSystemParser,
            bool appWasAutoRun)
        {
            _cacheSerializer = cacheSerializer;
            _fileSystemListener = fileSystemListener;
            _registryService = registryService;
            _fileSystemParser = fileSystemParser;
            _asyncFileSystemParser = asyncFileSystemParser;
            _fileSystemFilter = new FileSystemFilter();

            _fullCacheUpToDate = ReadFullCache(appWasAutoRun);

            //Listen to the changes in the whole system to update the fullCache.
            //This handler should be bound just after reading the full cache to ensure that _fullCache is initialized.
            _fileSystemListener.FolderSystemChanged += HandleFolderSystemChanged;
            _fileSystemListener.StartListening(null);

            //Parse file system fully (asynchronously).
            if (!_fullCacheUpToDate)
            {
                StartFileSystemParsing();
            }
        }

        #endregion

        #region Public Methods

        public List<FileSystemItem> GetSubFolders()
        {
            //Use synchronization to avoid conflicts with the cache update after expiration.
            lock (_cacheSync)
            {
                if (_filteredCacheItems == null)
                {
                    if (_fullCache != null && _fullCache.Items != null)
                    {
                        _filteredCacheItems = _fileSystemFilter.FilterItems(_fullCache.Items);
                    }
                }

                return _filteredCacheItems;
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
                _asyncFileSystemParser.Dispose();
            }

            // get rid of unmanaged resources
            try
            {
                lock (_cacheSync)
                {
                    SerializeFullCache();
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

        private void SerializeFullCache()
        {
            if (_fullCacheUpToDate)
            {
                _fullCache.LastFullScanTime = DateTime.Now;
            }
            _cacheSerializer.SerializeCache(_fullCache);
        }

        private void StartFileSystemParsing()
        {
            _asyncFileSystemParser.ParsingFinished += HandleParsingFinished;
            _asyncFileSystemParser.BeginParsing();
        }

        private void HandleParsingFinished(object sender, ItemEventArgs<FileSystemCache> e)
        {
            lock (_cacheSync)
            {
                _fullCache = e.Item;
                _fullCacheUpToDate = true;
                SerializeFullCache();

                ResetFilteredCacheItems();

                _asyncFileSystemParser.ParsingFinished -= HandleParsingFinished;
            }
        }

        private bool CacheUpToDate(FileSystemCache cache)
        {
            DateTime lastShutDownTime = _registryService.GetLastSystemShutDownTime();
            DateTime lastCacheWriteTime = cache.LastFullScanTime;

            TimeSpan timeDifference = lastShutDownTime - lastCacheWriteTime;
            bool cacheValid = timeDifference.TotalSeconds < CacheValidityPeriodInSeconds;
            return cacheValid;
        }

        private bool ReadFullCache(bool appWasAutoRun)
        {
            bool fullCacheUpToDate;

            //Parse file system.
            //We always keep the full cache in memory for the following reasons:
            //1. not to re-read it
            //2. the cache file may be deleted while this class is operating
            _fullCache = _cacheSerializer.DeserializeCache();
            if (_fullCache != null)
            {
                //The cache file can be up to date only if the current Navigation Assistant has been run on startup
                //and if it had been closed just on system shutdown and the current parser is created at application start.
                //In this case no additional folders can be created during NavAssistant being inactive.
                fullCacheUpToDate = CacheUpToDate(_fullCache) && appWasAutoRun;
            }
            else
            {
                //The application is loaded for the first time (no cache stored on disk).

                string cacheFolder = Path.GetDirectoryName(_cacheSerializer.CacheFilePath);
                bool cacheFolderExisted = Directory.Exists(cacheFolder);

                //Run this method in the main thread, thus freezing it.
                //Don't set any restrictions on this parsing, as want to grab the entire system.
                _fullCache = new FileSystemCache(_fileSystemParser.GetSubFolders(), DateTime.Now);
                _cacheSerializer.SerializeCache(_fullCache);

                //Updating the cache if cache folder has been created
                bool cacheFolderExists = Directory.Exists(cacheFolder);
                bool cacheFolderCreated = !cacheFolderExisted && cacheFolderExists;
                if (cacheFolderCreated)
                {
                    FileSystemChangeEventArgs e = new FileSystemChangeEventArgs(null, cacheFolder);
                    HandleFolderSystemChanged(this, e);
                }

                fullCacheUpToDate = true;
            }

            return fullCacheUpToDate;
        }

        private void HandleFolderSystemChanged(object sender, FileSystemChangeEventArgs e)
        {
            lock (_cacheSync)
            {
                Implementation.FileSystemParser.UpdateFolders(_fullCache.Items, e, null);

                //Though we write the cache back to file system in finalizer,
                //we try to write it periodically also (to minimize the effect of finalization errors,
                //e.g. when the computer is shut down unexpectedly).
                _updatesCount++;
                if (_updatesCount > _updatesCountToWrite)
                {
                    SerializeFullCache();
                    _updatesCount = 0;
                }

                //Cache may be null if settings (FoldersToParse, etc. have been changed before this update).
                if (_filteredCacheItems != null)
                {
                    Implementation.FileSystemParser.UpdateFolders(_filteredCacheItems, e, _fileSystemFilter.IsCorrect);
                }
            }
        }

        private void ResetFilteredCacheItems()
        {
            lock (_cacheSync)
            {
                _filteredCacheItems = null;
            }
        }

        #endregion
    }
}