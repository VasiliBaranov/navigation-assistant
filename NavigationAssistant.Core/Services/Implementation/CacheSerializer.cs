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
    /// </remarks>
    public class CacheSerializer : ICacheSerializer
    {
        #region Fields

        //Xml is too verbose
        //Binary formatting is not readable
        private readonly string _cacheFilePath;

        private const string Separator = "?";

        //Actually, different serializers can write to different paths,
        //but currently we syncronize them with one object (quick and dirty solution).
        private static readonly object CacheSync = new object();

        #endregion

        #region Constructors

        public CacheSerializer()
        {
            _cacheFilePath = Path.Combine(Application.CommonAppDataPath, "Cache.txt");
        }

        //For tests only
        public CacheSerializer(string cacheFilePath)
        {
            _cacheFilePath = cacheFilePath;
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
            List<string> lines = cache.Items.Select(GetLine).ToList();
            lines.Insert(0, cache.LastFullScanTime.ToString(CultureInfo.InvariantCulture));

            lock (CacheSync)
            {
                DirectoryUtility.EnsureFolder(Path.GetDirectoryName(_cacheFilePath));
                File.WriteAllLines(_cacheFilePath, lines.ToArray());
            }
        }

        public FileSystemCache DeserializeCache()
        {
            if (!File.Exists(_cacheFilePath))
            {
                return null;
            }

            string[] lines = File.ReadAllLines(_cacheFilePath);

            List<FileSystemItem> items = lines.Skip(1).Select(ParseLine).ToList();
            DateTime lastFullScanTime = DateTime.Parse(lines[0], CultureInfo.InvariantCulture);
            FileSystemCache cache = new FileSystemCache(items, lastFullScanTime);

            return cache;
        }

        public void DeleteCache()
        {
            lock (CacheSync)
            {
                if (File.Exists(_cacheFilePath))
                {
                    File.Delete(_cacheFilePath);
                }

                //Default cache location is c:\ProgramData\NavigationAssistant\NavigationAssistant\1.0.0.0\
                //So we have to delete three folders up to Local.
                string folderPath = Path.GetDirectoryName(_cacheFilePath);
                DirectoryUtility.DeleteUpperFolder(folderPath, 2);
            }
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
