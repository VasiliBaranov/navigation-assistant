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
        private const string Folder = "Temp";
        private const string SubFolder = "Temp\\1";
        private IFileSystemParser _parser;

        [SetUp]
        public void SetUp()
        {
            DirectoryUtility.EnsureClearFolder(Folder);

            _parser = new FileSystemParser(new FileSystemListener(true));
            _parser.FoldersToParse = new List<string> {Folder};
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(Folder, true);
        }

        [Test]
        public void GetSubFolders_ForEmptyFolder_ReturnsThisFolder()
        {
            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(1));
            Assert.That(subfolders[0].FullPath, Is.EqualTo(Path.GetFullPath(Folder)));
        }

        [Test]
        public void GetSubFolders_ForNonHiddenFolder_ReturnsNonHidden()
        {
            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(1));
            Assert.That(subfolders[0].IsHidden, Is.False);
        }

        [Test]
        public void GetSubFolders_ForHiddenFolder_ReturnsHidden()
        {
            DirectoryInfo folder = new DirectoryInfo(Folder);
            folder.Attributes = folder.Attributes | FileAttributes.Hidden;

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(1));
            Assert.That(subfolders[0].IsHidden, Is.True);
        }

        [Test]
        public void GetSubFolders_ForNonEmptyFolder_ReturnsAllSubFolders()
        {
            Directory.CreateDirectory(SubFolder);
            Directory.CreateDirectory(Folder + "\\1\\2");

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetSubFolders_FolderCreatedWhileOperation_FolderIncludedInOutput()
        {
            _parser = new FileSystemParserWithAction(new FileSystemListener(true), () => Directory.CreateDirectory(SubFolder));
            _parser.FoldersToParse = new List<string> { Folder };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));
            Assert.That(subfolders[1].FullPath, Is.EqualTo(Path.GetFullPath(SubFolder)));
        }

        [Test]
        public void GetSubFolders_FolderDeletedWhileOperation_FolderDeletedInOutput()
        {
            Directory.CreateDirectory(SubFolder);

            _parser = new FileSystemParserWithAction(new FileSystemListener(true), () => Directory.Delete(SubFolder, true));
            _parser.FoldersToParse = new List<string> { Folder };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(1));
            Assert.That(subfolders[0].FullPath, Is.EqualTo(Path.GetFullPath(Folder)));
        }

        [Test]
        public void GetSubFolders_FolderMadeHiddenWhileOperation_FolderIsHiddenInOutput()
        {
            Directory.CreateDirectory(SubFolder);
            _parser = new FileSystemParserWithAction(new FileSystemListener(true), MakeSubFolderHidden);
            _parser.FoldersToParse = new List<string> { Folder };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();
            FileSystemItem subfolder = DirectoryUtility.FindItem(subfolders, Path.GetFullPath(SubFolder));

            Assert.That(subfolder.IsHidden, Is.True);
        }

        [Test]
        public void GetSubFolders_FolderMadeVisibleWhileOperation_FolderIsVisibleInOutput()
        {
            Directory.CreateDirectory(SubFolder);
            MakeSubFolderHidden();

            _parser = new FileSystemParserWithAction(new FileSystemListener(true), MakeSubFolderVisible);
            _parser.FoldersToParse = new List<string> { Folder };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();
            FileSystemItem subfolder = DirectoryUtility.FindItem(subfolders, Path.GetFullPath(SubFolder));

            Assert.That(subfolder.IsHidden, Is.False);
        }

        [Test]
        public void GetSubFolders_FileCreatedWhileOperation_FileNotIncludedInOutput()
        {
            DirectoryUtility.EnsureClearFolder(SubFolder);
            _parser = new FileSystemParserWithAction(new FileSystemListener(true), () => File.WriteAllText(SubFolder + "\\temp.txt", "text"));
            _parser.FoldersToParse = new List<string> { Folder };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetSubFolders_FileUpdatedWhileOperation_FileNotIncludedInOutput()
        {
            DirectoryUtility.EnsureClearFolder(SubFolder);
            File.WriteAllText(SubFolder + "\\temp.txt", "text");
            _parser = new FileSystemParserWithAction(new FileSystemListener(true), () => File.WriteAllText(SubFolder + "\\temp.txt", "text"));
            _parser.FoldersToParse = new List<string> { Folder };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetSubFolders_FileDeletedWhileOperation_FileNotIncludedInOutput()
        {
            DirectoryUtility.EnsureClearFolder(SubFolder);
            File.WriteAllText(SubFolder + "\\temp.txt", "text");
            _parser = new FileSystemParserWithAction(new FileSystemListener(true), () => File.Delete(SubFolder + "\\temp.txt"));
            _parser.FoldersToParse = new List<string> { Folder };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetSubFolders_FileMadeHiddenWhileOperation_FileNotIncludedInOutput()
        {
            DirectoryUtility.EnsureClearFolder(SubFolder);
            File.WriteAllText(SubFolder + "\\temp.txt", "text");
            _parser = new FileSystemParserWithAction(new FileSystemListener(true),
                                                     () =>
                                                         {
                                                             FileInfo fileInfo = new FileInfo(SubFolder + "\\temp.txt");
                                                             fileInfo.Attributes = fileInfo.Attributes | FileAttributes.Hidden;
                                                         });
            _parser.FoldersToParse = new List<string> { Folder };

            List<FileSystemItem> subfolders = _parser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));
        }

        private static void MakeSubFolderHidden()
        {
            DirectoryInfo folder = new DirectoryInfo(SubFolder);
            folder.Attributes = folder.Attributes | FileAttributes.Hidden;
        }

        private static void MakeSubFolderVisible()
        {
            DirectoryInfo folder = new DirectoryInfo(SubFolder);
            folder.Attributes = folder.Attributes & ~FileAttributes.Hidden;
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
