﻿using System.Collections.Generic;
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
    }
}
