using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Tests
{
    /// <summary>
    /// Contains CachedFileSystemParser tests. Unfortunately, some of them may fail sometimes due to working with file system directly
    /// (e.g. if we wait not enough to handle file system event after folder creation).
    /// </summary>
    [TestFixture]
    public class CachedFileSystemParserTests
    {
        private CachedFileSystemParser _cachedFileSystemParser;

        private FileSystemListener _listener;
        private CacheSerializer _serializer;
        private FileSystemParserWithAction _fileSystemParser;
        private FakeRegistryService _registryService;
        private AsyncFileSystemParser _asyncParser;
        private bool _appRunOnStartup;
        private int _updatesCountToWrite;

        private const string FolderName = "Folder";
        private const string TempFileName = "Folder\\Temp\\File.txt";

        [SetUp]
        public void SetUp()
        {
            _listener = new FileSystemListener();
            _serializer = new CacheSerializer(TempFileName);
            //Don't use the same listener for parser and cached file parser, as they will operate on different threads
            _fileSystemParser = new FileSystemParserWithAction(new FileSystemListener(), new List<string> { FolderName });
            _registryService = new FakeRegistryService();
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }));
            _appRunOnStartup = true;
            _updatesCountToWrite = -1;

            DirectoryUtility.EnsureClearFolder(FolderName);

            if (Directory.Exists(FolderName))
            {
                DeleteFolder();
            }
            Directory.CreateDirectory(FolderName);
        }

        [TearDown]
        public void TearDown()
        {
            _cachedFileSystemParser.Dispose(); //to release any file system listening
            Thread.Sleep(50);

            DeleteFolder();
        }

        private static void DeleteFolder()
        {
            bool success = false;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Directory.Delete(FolderName, true);
                    success = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(1000); //to let system remove file watcher handlers off the folder
                }

                if (success)
                {
                    break;
                }
            }
        }

        private void CreateCachedParser()
        {
            if (_updatesCountToWrite == -1)
            {
                _cachedFileSystemParser = new CachedFileSystemParser(_fileSystemParser,
                                                                     _serializer,
                                                                     _listener,
                                                                     _registryService,
                                                                     _asyncParser,
                                                                     _appRunOnStartup);
            }
            else
            {
                _cachedFileSystemParser = new CachedFileSystemParser(_fileSystemParser,
                                                                     _serializer,
                                                                     _listener,
                                                                     _registryService,
                                                                     _asyncParser,
                                                                     _appRunOnStartup,
                                                                     _updatesCountToWrite);
            }
        }

        [Test]
        public void CreateCachedParser_NoCacheOnDiskWhileCreation_ParsedItemsReturned()
        {
            CreateCachedParser();

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(new List<string> { FolderName, "Temp" }));
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
            SetUpCache(DateTime.Now);

            CreateCachedParser();

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(new List<string> { "Cache1", "Cache2" }));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDate_CachedItemsReturnedBeforeParsing()
        {
            //Set up parsing delay at 1 minute, so it asynchronous parsing will not have been finished when calling GetSubFolders
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string>{FolderName}), 60);

            SetUpInactiveCache();

            CreateCachedParser();

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(new List<string> { "Cache1", "Cache2" }));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsUpToDateAndAppIsNotAutoRun_ParsedItemsReturnedAfterParsing()
        {
            //Start asynchronous parsing at once
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 0);
            _appRunOnStartup = false;

            SetUpCache(DateTime.Now);

            CreateCachedParser();

            //To wait for asynchronous parsing
            Thread.Sleep(200);
            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));

            List<string> expectedSubfolderNames = new List<string> { "Folder", "Temp" };
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(expectedSubfolderNames));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDate_ParsedItemsReturned()
        {
            //Start asynchronous parsing at once
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 0);

            SetUpInactiveCache();

            CreateCachedParser();

            //To wait for asynchronous parsing
            Thread.Sleep(200);
            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            Assert.That(subfolders.Count, Is.EqualTo(2));

            List<string> expectedSubfolderNames = new List<string> {"Folder", "Temp"};
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(expectedSubfolderNames));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDate_CacheSerializedAsActiveAfterParsing()
        {
            //Start asynchronous parsing at once
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 0);

            SetUpInactiveCache();

            CreateCachedParser();

            //To wait for asynchronous parsing
            Thread.Sleep(200);
            FileSystemCache updatedCache = _serializer.DeserializeCache();

            //Updated cache is not older than 2 seconds
            Assert.That(DateTime.Now - updatedCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 2)));

            List<string> expectedSubfolderNames = new List<string> { "Folder", "Temp" };
            Assert.That(GetFileSystemItemNames(updatedCache), Is.EquivalentTo(expectedSubfolderNames));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDateAndFileSystemUpdatedWhileParsing_CachedItemsReturnedBeforeParsingContainUpdate()
        {
            FileSystemParserWithAction parser = new FileSystemParserWithAction(new FileSystemListener(), new List<string> {FolderName});
            parser.Action = (() =>  Directory.CreateDirectory(FolderName + "\\Temp2"));
            parser.DelayInMilliseconds = 30*1000;
            _asyncParser = new AsyncFileSystemParser(parser, 0);

            SetUpInactiveCache();

            CreateCachedParser();

            //To wait for folder creation
            Thread.Sleep(200);
            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            List<string> expectedSubfolderNames = new List<string> { "Cache1", "Cache2", "Temp2" };
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(expectedSubfolderNames));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDateAndFileSystemUpdatedWhileParsing_ParsedItemsReturnedAfterParsingContainUpdate()
        {
            FileSystemParserWithAction parser = new FileSystemParserWithAction(new FileSystemListener(), new List<string> { FolderName });
            parser.Action = (() => Directory.CreateDirectory(FolderName + "\\Temp2"));
            parser.DelayInMilliseconds = 200;
            _asyncParser = new AsyncFileSystemParser(parser, 0);

            SetUpInactiveCache();

            _fileSystemParser.Action = (() => Directory.CreateDirectory(FolderName + "\\Temp2"));
            CreateCachedParser();

            //To wait for folder creation
            Thread.Sleep(400);
            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            List<string> expectedSubfolderNames = new List<string> { "Folder", "Temp", "Temp2" };
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(expectedSubfolderNames));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDateAndFilterUpdatedWhileParsing_CachedItemsReturnedBeforeParsingFilteredCorrectly()
        {
            //Set up parsing delay at 1 minute, so it asynchronous parsing will not have been finished when calling GetSubFolders
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 60);

            SetUpInactiveCache();

            CreateCachedParser();

            _cachedFileSystemParser.ExcludeFolderTemplates = new List<string> {"Cache1"};

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Count, Is.EqualTo(1));
            Assert.That(subfolders[0].Name, Is.EqualTo("Cache2"));

            _cachedFileSystemParser.FoldersToParse = new List<string> { FolderName + "\\Cache1" };
            subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDateAndFilterUpdatedWhileParsing_ParsedItemsReturnedAfterParsingFilteredCorrectly()
        {
            //Set up parsing delay at 1 minute, so it asynchronous parsing will not have been finished when calling GetSubFolders
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 0);

            SetUpInactiveCache();

            CreateCachedParser();

            //Wait until parsing is finished
            Thread.Sleep(200);
            _cachedFileSystemParser.ExcludeFolderTemplates = new List<string> { "Temp" };

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Count, Is.EqualTo(1));
            Assert.That(subfolders[0].Name, Is.EqualTo("Folder"));

            _cachedFileSystemParser.FoldersToParse = new List<string> { "Folder\\Temp\\" };
            subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFileSystemUpdated_ItemsReturnedContainUpdate()
        {
            _fileSystemParser.DelayInMilliseconds = 200;
            _fileSystemParser.Action = (() => Directory.CreateDirectory(FolderName + "\\Temp2"));
            CreateCachedParser();

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(new List<string> { "Folder", "Temp", "Temp2" }));
        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFilterUpdated_ItemsReturnedFilteredCorrectly()
        {
            CreateCachedParser();

            _cachedFileSystemParser.ExcludeFolderTemplates = new List<string> { "Temp" };

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Count, Is.EqualTo(1));
            Assert.That(subfolders[0].Name, Is.EqualTo("Folder"));

            _cachedFileSystemParser.FoldersToParse = new List<string> { "Folder\\Temp\\" };
            subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFileSystemUpdatedWhileParsingAndFilterUpdated_ItemsReturnedFilteredCorrectlyAndContainUpdate()
        {
            _fileSystemParser.DelayInMilliseconds = 200;
            _fileSystemParser.Action = (() => Directory.CreateDirectory(FolderName + "\\Temp2"));
            CreateCachedParser();

            _cachedFileSystemParser.ExcludeFolderTemplates = new List<string> { "Temp" };

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(new List<string> { "Folder", "Temp2" }));

            _cachedFileSystemParser.FoldersToParse = new List<string> { "Folder\\Temp\\" };
            subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFileSystemUpdatedAndFilterUpdated_ItemsReturnedFilteredCorrectlyAndContainUpdate()
        {
            CreateCachedParser();

            _cachedFileSystemParser.GetSubFolders();

            Directory.CreateDirectory("Folder\\Temp");
            Directory.CreateDirectory("Folder\\Temp\\asd");
            Directory.CreateDirectory("Folder\\Temp\\zxc");
            Thread.Sleep(200);

            _cachedFileSystemParser.FoldersToParse = new List<string> {"Folder\\Temp\\"};

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(new List<string> { "Temp", "asd", "zxc" }));

            _cachedFileSystemParser.ExcludeFolderTemplates = new List<string> {"asd"};
            subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(new List<string> { "Temp", "zxc" }));
        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFilterUpdatedAndFileSystemUpdated_ItemsReturnedFilteredCorrectlyAndContainUpdate()
        {
            Directory.CreateDirectory("Folder\\Temp");
            CreateCachedParser();

            _cachedFileSystemParser.ExcludeFolderTemplates = new List<string> { "Temp" };

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(new List<string> { "Folder" }));

            Directory.CreateDirectory("Folder\\Temp2\\asd");
            Thread.Sleep(200);

            subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(new List<string> { "Folder", "Temp2", "asd" }));
        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFileSystemUpdatedEnoughTimes_CacheIsSerializedAsActive()
        {
            CreateCachedParser();

            _cachedFileSystemParser.ExcludeFolderTemplates = new List<string> { "Temp" };

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Select(sf => sf.Name), Is.EquivalentTo(new List<string> { "Folder" }));

            Directory.CreateDirectory("Folder\\Temp2\\asd");
            Thread.Sleep(200);

            subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(GetFileSystemItemNames(subfolders), Is.EquivalentTo(new List<string> { "Folder", "Temp2", "asd" }));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskAndActiveAndFileSystemUpdatedEnoughTimes_CacheIsSerializedAsActive()
        {
            DateTime initialCacheDateTime = DateTime.Now;
            SetUpCache(initialCacheDateTime);

            _updatesCountToWrite = 1;
            CreateCachedParser();

            //To maintain significant time difference for timestamp
            Thread.Sleep(1000);

            //Temp folder will be created while initial cache serialization.
            Directory.CreateDirectory("Folder\\Temp2");
            Directory.CreateDirectory("Folder\\Temp3");

            //To let events be handled
            Thread.Sleep(200);

            FileSystemCache updatedCache = _serializer.DeserializeCache();

            //Updated cache is not older than 2 seconds
            Assert.That(updatedCache.LastFullScanTime > initialCacheDateTime, Is.True);

            Assert.That(GetFileSystemItemNames(updatedCache), Is.EquivalentTo(new List<string> { "Cache1", "Cache2", "Temp2", "Temp3" }));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskAndCacheIsNotUpToDateAndFileSystemUpdatedEnoughTimesWhileParsing_CacheIsSerializedAsInactive()
        {
            //Set up parsing delay at 1 minute, so it asynchronous parsing will not have been finished when creating folders
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 60);

            DateTime oldDateTime = DateTime.Now.AddDays(-1);
            SetUpCache(oldDateTime);

            _updatesCountToWrite = 1;
            CreateCachedParser();

            Directory.CreateDirectory("Folder\\Temp2");
            Directory.CreateDirectory("Folder\\Temp3");
            Thread.Sleep(200);

            FileSystemCache updatedCache = _serializer.DeserializeCache();

            //Updated cache is close to oldDateTime
            Assert.That(oldDateTime - updatedCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 2)));

            Assert.That(GetFileSystemItemNames(updatedCache), Is.EquivalentTo(new List<string> { "Cache1", "Cache2", "Temp2", "Temp3" }));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskAndCacheIsNotUpToDateAndFileSystemUpdatedEnoughTimesAfterParsing_CacheIsSerializedAsActive()
        {
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 0);

            SetUpInactiveCache();

            _updatesCountToWrite = 1;
            CreateCachedParser();

            //Wait while async parsing is finished
            Thread.Sleep(200);

            Directory.CreateDirectory("Folder\\Temp2");
            Directory.CreateDirectory("Folder\\Temp3");

            //Wait while folder creation is handled
            Thread.Sleep(200);

            FileSystemCache updatedCache = _serializer.DeserializeCache();

            //Updated cache is close to oldDateTime
            Assert.That(DateTime.Now - updatedCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 2)));

            Assert.That(GetFileSystemItemNames(updatedCache), Is.EquivalentTo(new List<string> { "Folder", "Temp", "Temp2", "Temp3" }));
        }

        [Test]
        public void Dispose_CacheOnDiskAndCacheIsNotUpToDateAndParsingFinished_CacheIsSerializedAsActive()
        {
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 0);

            SetUpInactiveCache();

            CreateCachedParser();

            //Wait while async parsing is finished
            Thread.Sleep(200);

            _cachedFileSystemParser.Dispose();

            FileSystemCache updatedCache = _serializer.DeserializeCache();
            Assert.That(DateTime.Now - updatedCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 2)));

            Assert.That(GetFileSystemItemNames(updatedCache), Is.EquivalentTo(new List<string> { "Folder", "Temp" }));
        }

        [Test]
        public void Dispose_CacheOnDiskAndCacheIsNotUpToDateAndParsingNotFinished_CacheIsSerializedAsInactive()
        {
            //Set up parsing delay at 1 minute, so it asynchronous parsing will not have been finished when creating folders
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 60);

            DateTime oldDateTime = DateTime.Now.AddDays(-1);
            SetUpCache(oldDateTime);

            CreateCachedParser();

            //Wait while async parsing is finished
            Thread.Sleep(200);

            _cachedFileSystemParser.Dispose();

            FileSystemCache updatedCache = _serializer.DeserializeCache();
            Assert.That(oldDateTime - updatedCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 2)));

            Assert.That(GetFileSystemItemNames(updatedCache), Is.EquivalentTo(new List<string> { "Cache1", "Cache2" }));
        }

        private void SetUpCache(DateTime cacheSaveTime)
        {
            List<FileSystemItem> items = new List<FileSystemItem>
                                             {
                                                 new FileSystemItem(FolderName + "\\Cache1"),
                                                 new FileSystemItem(FolderName + "\\Cache2"),
                                             };

            // Cache is one day older than the last shutdown
            
            FileSystemCache cache = new FileSystemCache(items, cacheSaveTime);
            _serializer.SerializeCache(cache);

            _registryService.LastSystemShutDownTime = DateTime.Now;
        }

        private void SetUpInactiveCache()
        {
            SetUpCache(DateTime.Now.AddDays(-1));
        }

        private static List<string> GetFileSystemItemNames(FileSystemCache updatedCache)
        {
            return GetFileSystemItemNames(updatedCache.Items);
        }

        private static List<string> GetFileSystemItemNames(List<FileSystemItem> items)
        {
            return items.Select(i => i.Name).ToList();
        }
    }
}
