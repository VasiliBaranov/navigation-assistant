using System;
using System.Collections.Generic;
using System.IO;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    /// <summary>
    /// Implements parsing file system folders with an effective cache.
    /// If there is no cache on disk, tries to parse it synchronously in the constructor.
    /// The cache is considered to be outdated if the app was not run on startup or
    /// if the cache had not been saved immediately before last turning off.
    /// If there is an outdated cache on disk, class registers file system re-parsing with a slight delay
    /// (to ease system loading). If the cache is active, doesn't rebuild it.
    /// </summary>
    /// <remarks>
    /// Use this class with Singleton lifetime from IoC
    /// </remarks>
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
        private bool _cacheUpToDate;

        //Should be synchronized
        private List<FileSystemItem> _filteredCacheItems;

        //Should be synchronized
        private FileSystemChanges _fileSystemChanges; 

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
            bool appRunOnStartup,
            int updatesCountToWrite)
            : this(fileSystemParser, cacheSerializer, fileSystemListener, registryService, asyncFileSystemParser, appRunOnStartup)
        {
            _updatesCountToWrite = updatesCountToWrite;
        }

        public CachedFileSystemParser(IFileSystemParser fileSystemParser,
            ICacheSerializer cacheSerializer,
            IFileSystemListener fileSystemListener,
            IRegistryService registryService,
            IAsyncFileSystemParser asyncFileSystemParser,
            bool appRunOnStartup)
        {
            _cacheSerializer = cacheSerializer;
            _fileSystemListener = fileSystemListener;
            _registryService = registryService;
            _fileSystemParser = fileSystemParser;
            _asyncFileSystemParser = asyncFileSystemParser;
            _fileSystemFilter = new FileSystemFilter();

            Initialize(appRunOnStartup);
        }

        #endregion

        #region Public Methods

        public List<FileSystemItem> GetSubFolders()
        {
            //Use synchronization to avoid conflicts with the cache update after expiration.
            lock (_cacheSync)
            {
                return _filteredCacheItems;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CachedFileSystemParser()
        {
            Dispose(false);
        }

        #endregion

        #region Non Public Methods

        private void Initialize(bool appRunOnStartup)
        {
            bool cacheFolderCreated;
            _filteredCacheItems = ReadCache(appRunOnStartup, out _cacheUpToDate, out cacheFolderCreated);
            _fileSystemChanges = new FileSystemChanges();
            if (cacheFolderCreated)
            {
                string cacheFolder = GetCacheFolder();
                FileSystemChangeEventArgs e = new FileSystemChangeEventArgs(null, cacheFolder);
                HandleFolderSystemChanged(this, e);
            }

            //Listen to the changes in the whole system to update the fullCache.
            //This handler should be bound just after reading the full cache to ensure that _fullCache is initialized.
            _fileSystemListener.FolderSystemChanged += HandleFolderSystemChanged;
            _fileSystemListener.StartListening(null);

            //Parse file system fully (asynchronously).
            if (!_cacheUpToDate)
            {
                StartFileSystemParsing();
            }
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
                    SerializeChanges();
                }
            }
            catch
            {
                // no exceptions in finalizer
            }
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
                FileSystemCache fullCache = e.Item;
                fullCache.LastFullScanTime = DateTime.Now;
                _cacheSerializer.SerializeCache(fullCache);

                _filteredCacheItems = FilterCache(fullCache);
                _fileSystemChanges = new FileSystemChanges();
                fullCache = null;
                GC.Collect();

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

        private List<FileSystemItem> ReadCache(bool appRunOnStartup, out bool fullCacheUpToDate, out bool cacheFolderCreated)
        {
            cacheFolderCreated = false;

            //Parse file system
            FileSystemCache fullCache = _cacheSerializer.DeserializeCache();
            if (fullCache != null)
            {
                //The cache file can be up to date only if the current Navigation Assistant has been run on startup
                //and if it had been closed just on system shutdown and the current parser is created at application start.
                //In this case no additional folders can be created during NavAssistant being inactive.
                fullCacheUpToDate = CacheUpToDate(fullCache) && appRunOnStartup;
            }
            else
            {
                //The application is loaded for the first time (no cache stored on disk).
                string cacheFolder = GetCacheFolder();
                bool cacheFolderExisted = Directory.Exists(cacheFolder);

                //Run this method in the main thread, thus freezing it.
                //Don't set any restrictions on this parsing, as want to grab the entire system.
                fullCache = new FileSystemCache(_fileSystemParser.GetSubFolders(), DateTime.Now);
                _cacheSerializer.SerializeCache(fullCache);

                //Updating the cache if cache folder has been created
                bool cacheFolderExists = Directory.Exists(cacheFolder);
                cacheFolderCreated = !cacheFolderExisted && cacheFolderExists;

                fullCacheUpToDate = true;
            }

            List<FileSystemItem> filteredCache = FilterCache(fullCache);
            fullCache = null;
            GC.Collect();

            return filteredCache;
        }

        private string GetCacheFolder()
        {
            return Path.GetDirectoryName(_cacheSerializer.CacheFilePath);
        }

        private void HandleFolderSystemChanged(object sender, FileSystemChangeEventArgs e)
        {
            lock (_cacheSync)
            {
                _fileSystemChanges.Changes.Add(e);

                //Though we write the cache back to file system in finalizer,
                //we try to write it periodically also (to minimize the effect of finalization errors,
                //e.g. when the computer is shut down unexpectedly).
                _updatesCount++;
                if (_updatesCount > _updatesCountToWrite)
                {
                    SerializeChanges();
                    _updatesCount = 0;
                }
                Implementation.FileSystemParser.UpdateFolders(_filteredCacheItems, e, _fileSystemFilter.IsCorrect);
            }
        }

        private void ResetFilteredCacheItems()
        {
            lock (_cacheSync)
            {
                SerializeChanges();

                //_cacheUpToDate will be the same. If it's up to date, it should be up to date; otherwise, not.
                FileSystemCache fullCache = _cacheSerializer.DeserializeCache();
                _filteredCacheItems = FilterCache(fullCache);

                fullCache = null;
                GC.Collect();
            }
        }

        private void SerializeChanges()
        {
            if (_cacheUpToDate)
            {
                _fileSystemChanges.CurrentTime = DateTime.Now;
                _cacheSerializer.SerializeCacheChanges(_fileSystemChanges);
            }
            _fileSystemChanges = new FileSystemChanges();
        }

        private List<FileSystemItem> FilterCache(FileSystemCache fullCache)
        {
            return (fullCache != null && fullCache.Items != null)
                       ? _fileSystemFilter.FilterItems(fullCache.Items)
                       : new List<FileSystemItem>();
        }

        #endregion
    }
}