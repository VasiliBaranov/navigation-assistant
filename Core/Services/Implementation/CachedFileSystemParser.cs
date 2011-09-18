using System;
using System.Collections.Generic;
using System.Web.Caching;
using Core.Model;

namespace Core.Services.Implementation
{
    public class CachedFileSystemParser : IFileSystemParser
    {
        #region Fields

        private const string CacheKey = "Folders";

        private readonly IFileSystemParser _fileSystemParser;

        private readonly Cache _cache;

        private readonly TimeSpan _cacheExpiration;

        #endregion

        #region Constructors

        public CachedFileSystemParser(IFileSystemParser fileSystemParser)
        {
            _fileSystemParser = fileSystemParser;
            _cache = new Cache();
            _cacheExpiration = new TimeSpan(0, 0, 10, 0); //10 minutes
        }

        #endregion

        #region Public Methods

        public List<FileSystemItem> GetFolders(string rootPath)
        {
            List<FileSystemItem> folders = _cache.Get(CacheKey) as List<FileSystemItem>;

            if (folders == null)
            {
                folders = _fileSystemParser.GetFolders(rootPath);

                DateTime absoluteExpiration = DateTime.Now.Add(_cacheExpiration);
                _cache.Add(CacheKey, folders, null, absoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }

            return folders;
        }

        #endregion
    }
}