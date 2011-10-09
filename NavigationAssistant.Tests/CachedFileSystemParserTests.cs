﻿using System;
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
            //Don't use the same listener for parser and cached file parser, as they will operate on different threads
            _fileSystemParser = new FileSystemParserWithAction(new FileSystemListener(), new List<string> { FolderName });
            _registryService = new FakeRegistryService();
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }));

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

        private void DeleteFolder()
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
            _cachedFileSystemParser = new CachedFileSystemParser(_fileSystemParser,
                                                     _serializer,
                                                     _listener,
                                                     _registryService,
                                                     _asyncParser);
        }

        private void CreateCachedParser(int updatesCountToWrite)
        {
            _cachedFileSystemParser = new CachedFileSystemParser(_fileSystemParser,
                                                     _serializer,
                                                     _listener,
                                                     _registryService,
                                                     _asyncParser,
                                                     updatesCountToWrite);
        }

        [Test]
        public void CreateCachedParser_NoCacheOnDiskWhileCreation_ParsedItemsReturned()
        {
            CreateCachedParser();

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            Assert.That(subfolders.Select(s => s.Name).ToList(), Is.EquivalentTo(new List<string> { FolderName, "Temp" }));
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

            //Updated cache is not older than 2 seconds
            Assert.That(DateTime.Now - updatedCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 2)));

            List<string> updatedCacheFolderNames = updatedCache.Items.Select(sf => sf.Name).ToList();
            List<string> expectedSubfolderNames = new List<string> { "Folder", "Temp" };
            Assert.That(updatedCacheFolderNames, Is.EquivalentTo(expectedSubfolderNames));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDateAndFileSystemUpdatedWhileParsing_CachedItemsReturnedBeforeParsingContainUpdate()
        {
            FileSystemParserWithAction parser = new FileSystemParserWithAction(new FileSystemListener(), new List<string> {FolderName});
            parser.Action = (() =>  Directory.CreateDirectory(FolderName + "\\Temp2"));
            parser.DelayInMilliseconds = 30*1000;
            _asyncParser = new AsyncFileSystemParser(parser, 0);

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

            //To wait for folder creation
            Thread.Sleep(200);
            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            List<string> updatedCacheFolderNames = subfolders.Select(sf => sf.Name).ToList();
            List<string> expectedSubfolderNames = new List<string> { "Cache1", "Cache2", "Temp2" };
            Assert.That(updatedCacheFolderNames, Is.EquivalentTo(expectedSubfolderNames));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDateAndFileSystemUpdatedWhileParsing_ParsedItemsReturnedAfterParsingContainUpdate()
        {
            FileSystemParserWithAction parser = new FileSystemParserWithAction(new FileSystemListener(), new List<string> { FolderName });
            parser.Action = (() => Directory.CreateDirectory(FolderName + "\\Temp2"));
            parser.DelayInMilliseconds = 200;

            _asyncParser = new AsyncFileSystemParser(parser, 0);

            List<FileSystemItem> items = new List<FileSystemItem>
                                             {
                                                 new FileSystemItem(FolderName + "\\Cache1"),
                                                 new FileSystemItem(FolderName + "\\Cache2"),
                                             };

            //Cache is one day older than the last shutdown
            FileSystemCache cache = new FileSystemCache(items, DateTime.Now.AddDays(-1));
            _serializer.SerializeCache(cache);

            _registryService.LastSystemShutDownTime = DateTime.Now;
            _fileSystemParser.Action = (() => Directory.CreateDirectory(FolderName + "\\Temp2"));
            CreateCachedParser();

            //To wait for folder creation
            Thread.Sleep(400);
            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();

            List<string> expectedSubfolderNames = new List<string> { "Folder", "Temp", "Temp2" };
            Assert.That(subfolders.Select(sf => sf.Name).ToList(), Is.EquivalentTo(expectedSubfolderNames));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskExistsAndIsNotUpToDateAndFilterUpdatedWhileParsing_CachedItemsReturnedBeforeParsingFilteredCorrectly()
        {
            //Set up parsing delay at 1 minute, so it asynchronous parsing will not have been finished when calling GetSubFolders
            _asyncParser = new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener(), new List<string> { FolderName }), 60);

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
            Assert.That(subfolders.Select(sf => sf.Name).ToList(), Is.EquivalentTo(new List<string> { "Folder", "Temp", "Temp2" }));
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
            Assert.That(subfolders.Select(sf => sf.Name).ToList(), Is.EquivalentTo(new List<string> { "Folder", "Temp2" }));

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
            Assert.That(subfolders.Select(sf => sf.Name).ToList(), Is.EquivalentTo(new List<string> { "Temp", "asd", "zxc" }));

            _cachedFileSystemParser.ExcludeFolderTemplates = new List<string> {"asd"};
            subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Select(sf => sf.Name).ToList(), Is.EquivalentTo(new List<string> { "Temp", "zxc" }));
        }

        [Test]
        public void GetSubFolders_NoCacheOnDiskAndFilterUpdatedAndFileSystemUpdated_ItemsReturnedFilteredCorrectlyAndContainUpdate()
        {
            Directory.CreateDirectory("Folder\\Temp");
            CreateCachedParser();

            _cachedFileSystemParser.ExcludeFolderTemplates = new List<string> { "Temp" };

            List<FileSystemItem> subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Select(sf => sf.Name), Is.EquivalentTo(new List<string> { "Folder" }));

            Directory.CreateDirectory("Folder\\Temp2\\asd");
            Thread.Sleep(200);

            subfolders = _cachedFileSystemParser.GetSubFolders();
            Assert.That(subfolders.Select(sf => sf.Name).ToList(), Is.EquivalentTo(new List<string> { "Folder", "Temp2", "asd" }));
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
            Assert.That(subfolders.Select(sf => sf.Name).ToList(), Is.EquivalentTo(new List<string> { "Folder", "Temp2", "asd" }));
        }

        [Test]
        public void GetSubFolders_CacheOnDiskAndActiveAndFileSystemUpdatedEnoughTimes_CacheIsSerializedAsActive()
        {
            CreateCachedParser(1);

            //Temp folder will be created while initial cache serialization.
            Directory.CreateDirectory("Folder\\Temp2");
            Thread.Sleep(200);

            FileSystemCache updatedCache = _serializer.DeserializeCache();

            //Updated cache is not older than 2 seconds
            Assert.That(DateTime.Now - updatedCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 2)));

            Assert.That(updatedCache.Items.Select(i => i.Name).ToList(),
                        Is.EquivalentTo(new List<string> {"Folder", "Temp", "Temp2"}));
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