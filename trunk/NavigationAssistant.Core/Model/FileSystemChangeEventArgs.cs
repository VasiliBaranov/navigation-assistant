using System;
using System.IO;

namespace NavigationAssistant.Core.Model
{
    public class FileSystemChangeEventArgs : EventArgs
    {
        private readonly string _oldFullPath;

        private readonly string _newFullPath;

        public FileSystemChangeEventArgs(string oldPath, string newPath)
        {
            if (oldPath != null)
            {
                _oldFullPath = Path.GetFullPath(oldPath);
            }
            if (newPath != null)
            {
                _newFullPath = Path.GetFullPath(newPath);
            }
        }

        public string OldFullPath
        {
            get { return _oldFullPath; }
        }

        public string NewFullPath
        {
            get { return _newFullPath; }
        }
    }
}
