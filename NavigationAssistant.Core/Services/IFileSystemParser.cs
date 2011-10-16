using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines methods for parsing of file system folders.
    /// </summary>
    public interface IFileSystemParser : IDisposable
    {
        /// <summary>
        /// Gets or sets the exclude folder templates; i.e. directories to be ignored while displaying matches, 
        /// e.g. "temp","Recycle Bin". Each entry is a case-sensitive regular expression.
        /// </summary>
        /// <value>
        /// The exclude folder templates.
        /// </value>
        List<string> ExcludeFolderTemplates { get; set; }

        /// <summary>
        /// Gets or sets the folders to parse (i.e. root folders) when searching for folder matches;
        /// e.g. "C:\Documents and Settings".
        /// </summary>
        /// <value>
        /// The folders to parse.
        /// </value>
        List<string> FoldersToParse { get; set; }

        List<FileSystemItem> GetSubFolders();
    }
}
