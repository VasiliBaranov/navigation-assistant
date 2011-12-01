using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    /// <summary>
    /// Implements file system cache serialization/deserialization.
    /// </summary>
    /// <remarks>
    /// Use this class with Singleton lifetime from IoC.
    /// Xml is too verbose, binary formatting is not readable
    /// </remarks>
    public class CacheSerializer : ICacheSerializer
    {
        #region Fields

        private readonly string _cacheFilePath;
        private readonly string _cacheChangesFilePath;
        private readonly string _cacheFolder;

        private const string Separator = "?";

        //Actually, different serializers can write to different paths,
        //but currently we syncronize them with one object (quick and dirty solution).
        private static readonly object CacheSync = new object();

        #endregion

        #region Constructors

        public CacheSerializer()
        {
            _cacheFolder = Application.CommonAppDataPath;
            _cacheFilePath = Path.Combine(_cacheFolder, "Cache.txt");
            _cacheChangesFilePath = Path.Combine(_cacheFolder, "CacheChanges.txt");
        }

        //For tests only
        public CacheSerializer(string cacheFilePath)
        {
            _cacheFolder = Path.GetDirectoryName(cacheFilePath);
            _cacheFilePath = cacheFilePath;
            _cacheChangesFilePath = Path.Combine(_cacheFolder, "CacheChanges.txt");
        }

        #endregion

        #region Properties

        public string CacheFilePath
        {
            get { return _cacheFilePath; }
        }

        #endregion

        #region Public Methods

        public void SerializeCache(FileSystemCache cache)
        {
            if (cache == null || cache.Items == null)
            {
                throw new ArgumentNullException("cache");
            }

            //File system path can not contain ?, so this format is not ambiguous
            List<string> lines = cache.Items.Select(GetCacheItemLine).ToList();
            lines.Insert(0, cache.LastFullScanTime.ToString(CultureInfo.InvariantCulture));

            lock (CacheSync)
            {
                DirectoryUtility.EnsureFolder(Path.GetDirectoryName(_cacheFilePath));
                File.WriteAllLines(_cacheFilePath, lines.ToArray());

                if (File.Exists(_cacheChangesFilePath))
                {
                    File.Delete(_cacheChangesFilePath);
                }
            }

            GC.Collect();
        }

        public FileSystemCache DeserializeCache()
        {
            if (!File.Exists(_cacheFilePath))
            {
                return null;
            }

            FileSystemCache cache;

            lock (CacheSync)
            {
                cache = ReadCache();
                if (File.Exists(_cacheChangesFilePath))
                {
                    FileSystemChanges changes = ReadChanges();

                    foreach (FileSystemChangeEventArgs fileSystemChangeArg in changes.Changes)
                    {
                        FileSystemParser.UpdateFolders(cache.Items, fileSystemChangeArg, null);
                    }
                    cache.LastFullScanTime = changes.CurrentTime;

                    SerializeCache(cache);
                }
            }

            GC.Collect();

            return cache;
        }

        public void SerializeCacheChanges(FileSystemChanges changes)
        {
            if (changes == null || changes.Changes == null)
            {
                throw new ArgumentNullException("changes");
            }

            lock (CacheSync)
            {
                DirectoryUtility.EnsureFolder(Path.GetDirectoryName(_cacheFilePath));
                string dateTime = changes.CurrentTime.ToString(CultureInfo.InvariantCulture);

                List<string> lines;
                if (File.Exists(_cacheChangesFilePath))
                {
                    lines = File.ReadAllLines(_cacheChangesFilePath).ToList();
                    lines[0] = dateTime;
                }
                else
                {
                    lines = new List<string> { dateTime };
                }

                List<string> linesToAdd = changes.Changes.Select(GetChangeItemLine).ToList();
                lines.AddRange(linesToAdd);

                File.WriteAllLines(_cacheChangesFilePath, lines.ToArray());
            }

            GC.Collect();
        }

        public void DeleteCache()
        {
            lock (CacheSync)
            {
                //TODO: Handle non-standard cache location case. May be move folder deletion to the app.
                //Default cache location is c:\ProgramData\NavigationAssistant\NavigationAssistant\1.0.0.0\
                //So we have to delete three folders up to Local.
                string folderPath = Path.GetDirectoryName(_cacheFilePath);
                DirectoryUtility.DeleteUpperFolder(folderPath, 2);
            }
        }

        #endregion

        #region Non Public Methods

        private FileSystemChanges ReadChanges()
        {
            string[] lines = File.ReadAllLines(_cacheChangesFilePath);

            List<FileSystemChangeEventArgs> items = lines.Skip(1).Select(ParseChangeItem).ToList();
            DateTime lastFullScanTime = DateTime.Parse(lines[0], CultureInfo.InvariantCulture);
            FileSystemChanges changes = new FileSystemChanges {Changes = items, CurrentTime = lastFullScanTime};

            return changes;
        }

        private FileSystemCache ReadCache()
        {
            string[] lines = File.ReadAllLines(_cacheFilePath);

            List<FileSystemItem> items = lines.Skip(1).Select(ParseCacheItem).ToList();
            DateTime lastFullScanTime = DateTime.Parse(lines[0], CultureInfo.InvariantCulture);
            FileSystemCache cache = new FileSystemCache(items, lastFullScanTime);

            return cache;
        }

        private static string GetChangeItemLine(FileSystemChangeEventArgs item)
        {
            return string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}", Separator, item.OldFullPath ?? string.Empty, item.NewFullPath ?? string.Empty);
        }

        private static string GetCacheItemLine(FileSystemItem item)
        {
            return string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}", Separator, item.FullPath ?? string.Empty, item.Name);
        }

        private static FileSystemChangeEventArgs ParseChangeItem(string line)
        {
            string[] substrings = line.Split(new[] {Separator}, StringSplitOptions.None);
            FileSystemChangeEventArgs e = new FileSystemChangeEventArgs(substrings[0], substrings[1]);
            return e;
        }

        private static FileSystemItem ParseCacheItem(string line)
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
