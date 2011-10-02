using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class CacheSerializer : ICacheSerializer
    {
        //Xml is too verbose
        //Binary formatting is not readable
        private readonly string _cacheFilePath;

        private const string Separator = "?";

        private static readonly object CacheSync = new object();

        public CacheSerializer(string cacheFolder)
        {
            _cacheFilePath = Path.Combine(cacheFolder, "Cache.txt");
        }

        public void SerializeCache(List<FileSystemItem> cache)
        {
            //File system path can not contain ?, so this format is not ambiguous
            string[] lines = cache.Select(GetLine).ToArray();

            lock (CacheSync)
            {
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
        private static string GetLine(FileSystemItem item)
        {
            int hiddenIndicator = item.IsHidden ? 1 : 0;
            return string.Format(CultureInfo.InvariantCulture, "{1}{0}{2}{0}{3}", Separator, item.FullPath, item.Name, hiddenIndicator);
        }

        private static FileSystemItem ParseLine(string line)
        {
            string[] parts = line.Split(new[] {Separator}, StringSplitOptions.None);

            FileSystemItem fileSystemItem = new FileSystemItem();
            fileSystemItem.FullPath = parts[0];

            if (parts.Length > 1)
            {
                fileSystemItem.Name = parts[1];
            }

            if (parts.Length > 2)
            {
                string hiddenIndicator = parts[2];
                if (string.IsNullOrEmpty(hiddenIndicator))
                {
                    fileSystemItem.IsHidden = false;
                }
                else
                {
                    fileSystemItem.IsHidden = string.Equals(hiddenIndicator, "1", StringComparison.Ordinal);
                }
            }

            return fileSystemItem;
        }
    }
}
