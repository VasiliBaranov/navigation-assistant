using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        #endregion

        #region Supplementary Fields

        private readonly object _cacheSync = new object();

        private List<string> _foldersToParseInitial;

        private const int UpdatesCountToWrite = 500;

        private const int CacheValidityPeriodInSeconds = 60*2;

        private readonly AsyncFileSystemParser _asyncFileSystemParser;

        #endregion

        #region Data Fields

        private List<string> _excludeFolderTemplates;

        private List<string> _foldersToParse;

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
                return _excludeFolderTemplates;
            }
            set
            {
                _excludeFolderTemplates = value;
                ResetFilteredCacheItems();
            }
        }

        public List<string> FoldersToParse
        {
            get
            {
                return _foldersToParseInitial;
            }
            set
            {
                _foldersToParse = NormalizeFolders(value);
                _foldersToParseInitial = value;
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

        public AsyncFileSystemParser AsyncFileSystemParser
        {
            get { return _asyncFileSystemParser; }
        }

        #endregion

        #region Constructors

        public CachedFileSystemParser(IFileSystemParser fileSystemParser,
            ICacheSerializer cacheSerializer,
            IFileSystemListener fileSystemListener,
            IRegistryService registryService,
            AsyncFileSystemParser asyncFileSystemParser)
        {
            _cacheSerializer = cacheSerializer;
            _fileSystemListener = fileSystemListener;
            _registryService = registryService;
            _fileSystemParser = fileSystemParser;
            _asyncFileSystemParser = asyncFileSystemParser;

            _fullCacheUpToDate = ReadFullCache();
            //_fullCache = new FileSystemCache(new List<FileSystemItem>(), DateTime.Now);

            //Listen to the changes in the whole system to update the fullCache.
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
                        _filteredCacheItems = FileSystemFilterUtility.FilterCache(_fullCache.Items,
                                                                                  _foldersToParse,
                                                                                  _excludeFolderTemplates);
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
            }

            // get rid of unmanaged resources
            try
            {
                lock (_cacheSync)
                {
                    if (_fullCacheUpToDate)
                    {
                        _fullCache.LastFullScanTime = DateTime.Now;
                    }
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
                _cacheSerializer.SerializeCache(_fullCache);
                _fullCacheUpToDate = true;

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

        private bool ReadFullCache()
        {
            bool fullCacheUpToDate;

            //Parse file system.
            //We always keep the full cache in memory for the following reasons:
            //1. not to re-read it
            //2. the cache file may be deleted while this class is operating
            _fullCache = _cacheSerializer.DeserializeCache();
            if (_fullCache != null)
            {
                fullCacheUpToDate = CacheUpToDate(_fullCache);
            }
            else
            {
                //The application is loaded for the first time (no cache stored on disk).

                //Run this method in the main thread, thus freezing it.
                //Don't set any restrictions on this parsing, as want to grab the entire system.
                _fullCache = new FileSystemCache(_fileSystemParser.GetSubFolders(), DateTime.Now);
                _cacheSerializer.SerializeCache(_fullCache);
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
                if (_updatesCount > UpdatesCountToWrite)
                {
                    _cacheSerializer.SerializeCache(_fullCache);
                    _updatesCount = 0;
                }

                //Cache may be null if settings (FoldersToParse, etc. have been changed before this update).
                if (_filteredCacheItems != null)
                {
                    Implementation.FileSystemParser.UpdateFolders(_filteredCacheItems, e, IsCorrect);
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

        private bool IsCorrect(FileSystemItem item)
        {
            List<Regex> excludeRegexes = FileSystemFilterUtility.GetExcludeRegexes(_excludeFolderTemplates);
            return FileSystemFilterUtility.IsCorrect(item, _foldersToParse, excludeRegexes);
        }

        private static List<string> NormalizeFolders(IEnumerable<string> folders)
        {
            return folders
                .Select(StringUtility.MakeFirstLetterUppercase)
                .Select(s=>s.TrimEnd('\\')).ToList();
        }

        #endregion
    }
}