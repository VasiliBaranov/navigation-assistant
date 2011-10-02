using System;
using System.IO;

namespace NavigationAssistant.Core.Model
{
    public class FileSystemChangeEventArgs : EventArgs
    {
        private readonly string _oldPath;

        private readonly string _newPath;

        public FileSystemChangeEventArgs(string oldPath, string newPath)
        {
            _oldPath = oldPath;
            _newPath = newPath;
        }

        public string OldPath
        {
            get { return _oldPath; }
        }

        public string NewPath
        {
            get { return _newPath; }
        }
    }
}
