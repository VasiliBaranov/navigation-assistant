using System;
using System.Collections.Generic;

namespace NavigationAssistant.Core.Model
{
    /// <summary>
    /// Represents a cache of parsed file system.
    /// </summary>
    public class FileSystemCache
    {
        public List<FileSystemItem> Items { get; set; }

        public DateTime LastFullScanTime { get; set; }

        public FileSystemCache(List<FileSystemItem> items, DateTime lastFullScanTime)
        {
            Items = items;
            LastFullScanTime = lastFullScanTime;
        }
    }
}
