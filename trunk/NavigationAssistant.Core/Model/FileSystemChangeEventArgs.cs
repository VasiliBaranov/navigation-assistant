using System;
using System.IO;

namespace NavigationAssistant.Core.Model
{
    /// <summary>
    /// Provides data for the FileSystemChanged event.
    /// </summary>
    /// <remarks>
    /// See FileSystemItem remarks.
    /// </remarks>
    public class FileSystemChangeEventArgs : EventArgs
    {
        #region Fields

        private readonly string _oldFullPath;

        private readonly string _newFullPath;

        #endregion

        #region Constructors

        /// <exception cref="PathTooLongException"></exception>
        public FileSystemChangeEventArgs(string oldPath, string newPath)
        {
            if (!string.IsNullOrEmpty(oldPath))
            {
                _oldFullPath = Path.GetFullPath(oldPath);
            }
            if (!string.IsNullOrEmpty(newPath))
            {
                _newFullPath = Path.GetFullPath(newPath);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the old full path for the folder changed. If it's null, the folder has been created.
        /// </summary>
        public string OldFullPath
        {
            get { return _oldFullPath; }
        }

        /// <summary>
        /// Gets the new full path for the folder changed. If it's null, the folder has been deleted.
        /// </summary>
        public string NewFullPath
        {
            get { return _newFullPath; }
        }

        #endregion
    }
}
