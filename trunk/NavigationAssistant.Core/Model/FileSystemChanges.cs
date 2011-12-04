using System;
using System.Collections.Generic;

namespace NavigationAssistant.Core.Model
{
    public class FileSystemChanges
    {
        public List<FileSystemChangeEventArgs> Changes { get; set; }

        public DateTime LastFullScanTime { get; set; }

        public FileSystemChanges()
        {
            Changes = new List<FileSystemChangeEventArgs>();
        }
    }
}
