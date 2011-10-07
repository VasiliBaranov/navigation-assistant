using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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

        #endregion

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
