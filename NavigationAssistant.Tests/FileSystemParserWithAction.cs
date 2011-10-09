using System;
using System.Collections.Generic;
using System.Threading;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;

namespace NavigationAssistant.Tests
{
    public class FileSystemParserWithAction : FileSystemParser
    {
        #region Fields

        private Action _action;

        private int _delayInMilliseconds;

        #endregion

        #region Properties

        public Action Action
        {
            get { return _action; }
            set { _action = value; }
        }

        public int DelayInMilliseconds
        {
            get { return _delayInMilliseconds; }
            set { _delayInMilliseconds = value; }
        }

        #endregion

        #region Constructors

        public FileSystemParserWithAction(IFileSystemListener fileSystemListener, List<string> foldersToParse) : base(fileSystemListener, foldersToParse)
        {

        }

        public FileSystemParserWithAction(IFileSystemListener fileSystemListener, Action action)
            : base(fileSystemListener)
        {
            _action = action;
            _delayInMilliseconds = _action != null ? 200 : 0;
        }

        #endregion

        #region Non Public Methods

        protected override List<FileSystemItem> ProcessSubFolders(List<FileSystemItem> subfolders)
        {
            if (_action != null)
            {
                _action();
            }
            Thread.Sleep(_delayInMilliseconds); //To let file system handlers handle the action

            return subfolders;
        }

        #endregion
    }
}
