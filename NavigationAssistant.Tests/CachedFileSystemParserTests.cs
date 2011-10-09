using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class CachedFileSystemParserTests
    {
        private CachedFileSystemParser _cachedFileSystemParser;

        private FileSystemListener _listener;
        private CacheSerializer _serializer;
        private FileSystemParserWithAction _fileSystemParser;
        private FakeRegistryService _registryService;
        private AsyncFileSystemParser _asyncParser;

        private const string FolderName = "Folder";
        private const string TempFolderName = "Folder\\Temp";
        private const string TempFileName = "Folder\\Temp\\File.txt";

        [SetUp]
        public void SetUp()
        {
            _listener = new FileSystemListener();
            _serializer = new CacheSerializer(TempFileName);
            //Don't use the same listener parser and cached file parser, as they will operate on different threads
            _fileSystemParser = new FileSystemParserWithAction(new FileSystemListener(), new List<string> { FolderName });
            _registryService = new FakeRegistryService();
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(_listener, new List<string> { FolderName }));

            DirectoryUtility.EnsureClearFolder(FolderName);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(FolderName, true);
        }

        private void CreateCachedParser()
        {
            _cachedFileSystemParser = new CachedFileSystemParser(_fileSystemParser,
                                                     _serializer,
                                                     _listener,
                                                     _registryService,
                                                     _asyncParser);
        }

        [Test]
        public void CreateCachedParser_NoCacheOnDiskWhileCreation_ParsedItemsReturned()
        {
            CreateCachedParser();

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(1));
            Assert.That(subfolders[0].Name, Is.EqualTo(FolderName));
        }

        [Test]
        public void CreateCachedParser_NoCacheOnDiskWhileCreation_CacheSerialized()
        {
            CreateCachedParser();

            Assert.That(File.Exists(TempFileName), Is.True);
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsUpToDate_CachedItemsReturned()
        {
            List<FileSystemItem> items = new List<FileSystemItem>
                                             {
                                                 new FileSystemItem(FolderName + "\\Cache1"),
                                                 new FileSystemItem(FolderName + "\\Cache2"),
                                             };
            FileSystemCache cache = new FileSystemCache(items, DateTime.Now);
            _serializer.SerializeCache(cache);
            _registryService.LastSystemShutDownTime = DateTime.Now;

            CreateCachedParser();

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));
            Assert.That(subfolders[0].Name, Is.EqualTo("Cache1"));
            Assert.That(subfolders[1].Name, Is.EqualTo("Cache2"));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDate_CachedItemsReturnedBeforeParsing()
        {
            //Set up parsing delay at 1 minute, so it asynchronous parsing will not have been finished when calling GetSubFolders
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string>{FolderName}), 60);

            List<FileSystemItem> items = new List<FileSystemItem>
                                             {
                                                 new FileSystemItem(FolderName + "\\Cache1"),
                                                 new FileSystemItem(FolderName + "\\Cache2"),
                                             };
            //Cache is one day older than the last shutdown
            FileSystemCache cache = new FileSystemCache(items, DateTime.Now.AddDays(-1));
            _serializer.SerializeCache(cache);

            _registryService.LastSystemShutDownTime = DateTime.Now;

            CreateCachedParser();

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));
            Assert.That(subfolders[0].Name, Is.EqualTo("Cache1"));
            Assert.That(subfolders[1].Name, Is.EqualTo("Cache2"));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDate_ParsedItemsReturned()
        {
            //Start asynchronous parsing at once
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 0);

            List<FileSystemItem> items = new List<FileSystemItem>
                                             {
                                                 new FileSystemItem(FolderName + "\\Cache1"),
                                                 new FileSystemItem(FolderName + "\\Cache2"),
                                             };

            //Cache is one day older than the last shutdown
            FileSystemCache cache = new FileSystemCache(items, DateTime.Now.AddDays(-1));
            _serializer.SerializeCache(cache);

            _registryService.LastSystemShutDownTime = DateTime.Now;

            CreateCachedParser();

            //To wait for asynchronous parsing
            Thread.Sleep(200);
            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));

            List<string> subfolderNames = subfolders.Select(sf => sf.Name).ToList();
            List<string> expectedSubfolderNames = new List<string> {"Folder", "Temp"};
            Assert.That(subfolderNames, Is.EquivalentTo(expectedSubfolderNames));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDate_CacheSerializedAsActiveAfterParsing()
        {
            //Start asynchronous parsing at once
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 0);

            List<FileSystemItem> items = new List<FileSystemItem>
                                             {
                                                 new FileSystemItem(FolderName + "\\Cache1"),
                                                 new FileSystemItem(FolderName + "\\Cache2"),
                                             };

            //Cache is one day older than the last shutdown
            FileSystemCache cache = new FileSystemCache(items, DateTime.Now.AddDays(-1));
            _serializer.SerializeCache(cache);

            _registryService.LastSystemShutDownTime = DateTime.Now;

            CreateCachedParser();

            //To wait for asynchronous parsing
            Thread.Sleep(200);
            FileSystemCache updatedCache = _serializer.DeserializeCache();

            //Updated cache is not older than 1 second
            Assert.That(DateTime.Now - updatedCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 1)));

            List<string> updatedCacheFolderNames = updatedCache.Items.Select(sf => sf.Name).ToList();
            List<string> expectedSubfolderNames = new List<string> { "Folder", "Temp" };
            Assert.That(updatedCacheFolderNames, Is.EquivalentTo(expectedSubfolderNames));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDateAndFileSystemUpdatedWhileParsing_CachedItemsReturnedBeforeParsingContainUpdate()
        {

        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDateAndFileSystemUpdatedWhileParsing_ParsedItemsReturnedAfterParsingContainUpdate()
        {

        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDateAndFilterUpdatedWhileParsing_ParsedItemsReturnedAfterParsingFilteredCorrectly()
        {

        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFileSystemUpdated_ItemsReturnedContainUpdate()
        {

        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFilterUpdated_ItemsReturnedFilteredCorrectly()
        {

        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFileSystemUpdatedAndFilterUpdated_ItemsReturnedFilteredCorrectlyAndContainUpdate()
        {

        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFilterUpdatedAndFileSystemUpdated_ItemsReturnedFilteredCorrectlyAndContainUpdate()
        {

        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFileSystemUpdatedEnoughTimes_CacheIsSerializedAsActive()
        {

        }

        [Test]
        public void GetSubFolders_CacheOnDiskAndActiveAndFileSystemUpdatedEnoughTimes_CacheIsSerializedAsActive()
        {

        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndCacheIsNotUpToDateAndFileSystemUpdatedEnoughTimesWhileParsing_CacheIsSerializedAsInactive()
        {

        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndCacheIsNotUpToDateAndFileSystemUpdatedEnoughTimesAfterParsing_CacheIsSerializedAsActive()
        {

        }

        [Test]
        public void Dispose_NoCacheOnDiskAndCacheIsNotUpToDateAndParsingFinished_CacheIsSerializedAsActive()
        {

        }

        [Test]
        public void Dispose_NoCacheOnDiskAndCacheIsNotUpToDateAndParsingNotFinished_CacheIsSerializedAsInactive()
        {

        }
    }
}
