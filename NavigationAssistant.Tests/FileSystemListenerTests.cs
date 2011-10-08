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
    public class FileSystemListenerTests
    {
        private const string FolderName = "Folder";
        private const string TempFolderName = "Folder\\Temp";
        private const string TempFileName = "Folder\\Temp\\File.txt";

        private IFileSystemListener _fileSystemListener;

        private FileSystemChangeEventArgs _eventArgs;

        [SetUp]
        public void SetUp()
        {
            _fileSystemListener = new FileSystemListener();
            _fileSystemListener.FolderSystemChanged += HandleFolderSystemChanged;
            _eventArgs = null;

            DirectoryUtility.EnsureClearFolder(FolderName);
        }

        [TearDown]
        public void TearDown()
        {
            _fileSystemListener.Dispose();

            Directory.Delete(FolderName, true);
        }

        [Test]
        public void StartListening_FolderCreated_EventFired()
        {
            _fileSystemListener.StartListening(new List<string> { FolderName });

            Directory.CreateDirectory(TempFolderName);

            Thread.Sleep(200);

            Assert.That(_eventArgs, Is.Not.Null);
            Assert.That(_eventArgs.OldFullPath, Is.Null);
            Assert.That(_eventArgs.NewFullPath, Is.EqualTo(Path.GetFullPath(TempFolderName)));
        }

        [Test]
        public void StartListening_FolderDeleted_EventFired()
        {
            Directory.CreateDirectory(TempFolderName);

            _fileSystemListener.StartListening(new List<string> { FolderName });

            Directory.Delete(TempFolderName);
            Thread.Sleep(200);

            Assert.That(_eventArgs, Is.Not.Null);
            Assert.That(_eventArgs.OldFullPath, Is.EqualTo(Path.GetFullPath(TempFolderName)));
            Assert.That(_eventArgs.NewFullPath, Is.Null);
        }

        [Test]
        public void StartListening_FolderMadeHidden_EventNotFired()
        {
            Directory.CreateDirectory(TempFolderName);

            _fileSystemListener.StartListening(new List<string> { FolderName });

            Utility.MakeFolderHidden(TempFolderName);
            Thread.Sleep(200);

            Assert.That(_eventArgs, Is.Null);
        }

        [Test]
        public void StartListening_FolderMadeVisible_EventNotFired()
        {
            Directory.CreateDirectory(TempFolderName);
            Utility.MakeFolderHidden(TempFolderName);

            _fileSystemListener.StartListening(new List<string> { FolderName });

            Utility.MakeFolderVisible(TempFolderName);
            Thread.Sleep(200);

            Assert.That(_eventArgs, Is.Null);
        }

        [Test]
        public void StartListening_FileCreated_EventNotFired()
        {
            Directory.CreateDirectory(TempFolderName);
            _fileSystemListener.StartListening(new List<string> { FolderName });

            File.WriteAllText(TempFileName, string.Empty);

            Thread.Sleep(200);

            Assert.That(_eventArgs, Is.Null);
        }

        [Test]
        public void StartListening_FileDeleted_EventNotFired()
        {
            Directory.CreateDirectory(TempFolderName);
            File.WriteAllText(TempFileName, string.Empty);

            _fileSystemListener.StartListening(new List<string> { FolderName });

            File.Delete(TempFileName);
            Thread.Sleep(200);

            Assert.That(_eventArgs, Is.Null);
        }

        [Test]
        public void StartListening_FileMadeHidden_EventNotFired()
        {
            Directory.CreateDirectory(TempFolderName);
            File.WriteAllText(TempFileName, string.Empty);

            _fileSystemListener.StartListening(new List<string> { FolderName });

            Utility.MakeFileHidden(TempFileName);
            Thread.Sleep(200);

            Assert.That(_eventArgs, Is.Null);
        }

        [Test]
        public void StartListening_FileMadeVisible_EventNotFired()
        {
            Directory.CreateDirectory(TempFolderName);
            File.WriteAllText(TempFileName, string.Empty);
            Utility.MakeFileHidden(TempFileName);

            _fileSystemListener.StartListening(new List<string> { FolderName });

            Utility.MakeFileVisible(TempFileName);
            Thread.Sleep(200);

            Assert.That(_eventArgs, Is.Null);
        }

        [Test]
        public void StartListening_FolderCreatedOutsideListened_EventNotFired()
        {
            Directory.CreateDirectory(TempFolderName);
            _fileSystemListener.StartListening(new List<string> { TempFolderName });

            Directory.CreateDirectory(FolderName + "\\Temp2");
            Thread.Sleep(200);

            Assert.That(_eventArgs, Is.Null);
        }

        private void HandleFolderSystemChanged(object sender, FileSystemChangeEventArgs e)
        {
            _eventArgs = e;
        }
    }
}
