using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class FileSystemParserTests
    {
        private const string FolderName = "Temp";
        private IFileSystemParser _parser;

        [SetUp]
        public void SetUp()
        {
            DirectoryUtility.EnsureClearFolder(FolderName);

            _parser = new FileSystemParser(new FileSystemListener());
            _parser.FoldersToParse = new List<string> {FolderName};
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(FolderName, true);
        }

        [Test]
        public void GetSubFolders_ForEmptyFolder_ReturnsThisFolder()
        {
            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(1));
            Assert.That(subfolders[0].FullPath, Is.EqualTo(Path.GetFullPath(FolderName)));
        }

        [Test]
        public void GetSubFolders_ForNonEmptyFolder_ReturnsAllSubFolders()
        {
            Directory.CreateDirectory(FolderName + "\\1");
            Directory.CreateDirectory(FolderName + "\\1\\2");

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetSubFolders_FolderCreatedWhileOperation_FolderIncludedInOutput()
        {
            _parser = new FileSystemParserWithAction(new FileSystemListener(), ()=>Directory.CreateDirectory(FolderName + "\\1"));
            _parser.FoldersToParse = new List<string> { FolderName };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));
            Assert.That(subfolders[1].FullPath, Is.EqualTo(Path.GetFullPath(FolderName + "\\1")));
        }

        [Test]
        public void GetSubFolders_FolderDeletedWhileOperation_FolderDeletedInOutput()
        {
            const string newFolderName = FolderName + "\\1";
            Directory.CreateDirectory(newFolderName);

            _parser = new FileSystemParserWithAction(new FileSystemListener(), () => Directory.Delete(newFolderName, true));
            _parser.FoldersToParse = new List<string> { FolderName };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(1));
            Assert.That(subfolders[0].FullPath, Is.EqualTo(Path.GetFullPath(FolderName)));
        }

        protected class FileSystemParserWithAction : FileSystemParser
        {
            private readonly Action _action;

            public FileSystemParserWithAction(IFileSystemListener fileSystemListener, Action action) : base(fileSystemListener)
            {
                _action = action;
            }

            protected override List<FileSystemItem> ProcessSubFolders(List<FileSystemItem> subfolders)
            {
                _action();
                Thread.Sleep(200); //To let file system handlers handle the action.

                return subfolders;
            }
        }
    }
}
