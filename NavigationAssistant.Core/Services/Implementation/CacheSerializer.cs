using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    //Use this class with Singleton lifetime from IoC
    public class CacheSerializer : ICacheSerializer
    {
        #region Fields

        //Xml is too verbose
        //Binary formatting is not readable
        private string _cacheFilePath;

        private const string Separator = "?";

        //Actually, different serializers can write to different paths,
        //but currently we syncronize them with one object (quick and dirty solution).
        private static readonly object CacheSync = new object();

        #endregion

        #region Constructors

        public CacheSerializer(string cacheFolder)
        {
            _cacheFilePath = Path.Combine(cacheFolder, "Cache.txt");
        }

        #endregion

        public string CacheFolder
        {
            get
            {
                return Path.GetDirectoryName(_cacheFilePath);
            }
            set
            {
                string oldCacheFolder = Path.GetFullPath(Path.GetDirectoryName(_cacheFilePath));
                bool folderChanged = !string.Equals(Path.GetFullPath(value), oldCacheFolder, StringComparison.Ordinal);
                if (folderChanged)
                {
                    lock (CacheSync)
                    {
                        if (File.Exists(_cacheFilePath))
                        {
                            File.Delete(_cacheFilePath);
                        }
                    }
                }

                _cacheFilePath = Path.Combine(value, "Cache.txt");
            }
        }

        #region Public Methods

        public void SerializeCache(List<FileSystemItem> cache)
        {
            //File system path can not contain ?, so this format is not ambiguous
            string[] lines = cache.Select(GetLine).ToArray();

            lock (CacheSync)
            {
                DirectoryUtility.EnsureFolder(Path.GetDirectoryName(_cacheFilePath));
                File.WriteAllLines(_cacheFilePath, lines);
            }
        }

        public List<FileSystemItem> DeserializeCache()
        {
            if (!File.Exists(_cacheFilePath))
            {
                return null;
            }

            string[] lines = File.ReadAllLines(_cacheFilePath);

            List<FileSystemItem> result = lines.Select(ParseLine).ToList();

            return result;
        }

        #endregion

        #region Non Public Methods

        private static string GetLine(FileSystemItem item)
        {
            return string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}", Separator, item.FullPath, item.Name);
        }

        private static FileSystemItem ParseLine(string line)
        {
            int separatorIndex = line.IndexOf(Separator);

            if (separatorIndex < 0)
            {
                return new FileSystemItem(string.Empty, line);
            }

            string itemPath = line.Substring(0, separatorIndex);
            string itemName = string.Empty;

            if (line.Length > separatorIndex + 1)
            {
                itemName = line.Substring(separatorIndex + 1);
            }

            return new FileSystemItem(itemName, itemPath);
        }

        #endregion
    }
}
