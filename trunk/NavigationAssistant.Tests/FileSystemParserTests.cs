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
            _parser = new FileSystemParserWithDelay(new FileSystemListener());
            _parser.FoldersToParse = new List<string> { FolderName };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));
            Assert.That(subfolders[1].FullPath, Is.EqualTo(Path.GetFullPath(FolderName + "\\1")));
        }

        public void GetSubFolders_FolderRenamedWhileOperation_FolderRenamedInOutput()
        {

        }

        public void GetSubFolders_FolderDeletedWhileOperation_FolderDeletedInOutput()
        {

        }

        protected class FileSystemParserWithDelay : FileSystemParser
        {
            public FileSystemParserWithDelay(IFileSystemListener fileSystemListener) : base(fileSystemListener)
            {

            }

            protected override List<FileSystemItem> ProcessSubFolders(List<FileSystemItem> subfolders)
            {
                Directory.CreateDirectory(FolderName + "\\1");
                Thread.Sleep(200);

                return subfolders;
            }
        }
    }
}
