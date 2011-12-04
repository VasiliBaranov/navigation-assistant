using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class CacheSerializerTests
    {
        private const string Folder = "Temp";
        private const string CacheFilePath = "Temp\\Cache.txt";
        private ICacheSerializer _serializer;

        [SetUp]
        public void SetUp()
        {
            DirectoryUtility.EnsureClearFolder(Folder);

            _serializer = new CacheSerializer(CacheFilePath);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(Folder, true);
        }

        [Test]
        public void DeserializeCache_FromSerializedCache_CacheIsCorrect()
        {
            //Arrange
            List<FileSystemItem> items = new List<FileSystemItem>
                                             {
                                                 new FileSystemItem("name1", "folder1"),
                                                 new FileSystemItem("C:\\")

                                             };
            DateTime dateTime = DateTime.Now;
            FileSystemCache cache = new FileSystemCache(items, dateTime);

            _serializer.SerializeCache(cache);

            //Act
            FileSystemCache actualCache = _serializer.DeserializeCache();

            //Assert
            //Serialization does not preserve milliseconds, so we use epsilon check.
            Assert.That(dateTime - actualCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 1)));
            Assert.That(actualCache.Items.Count, Is.EqualTo(2));

            Assert.That(actualCache.Items[0].FullPath, Is.EqualTo(Path.GetFullPath("folder1")));
            Assert.That(actualCache.Items[0].Name, Is.EqualTo("name1"));

            Assert.That(actualCache.Items[1].FullPath, Is.EqualTo(Path.GetFullPath("C:\\")));
            Assert.That(actualCache.Items[1].Name, Is.EqualTo(""));
        }

        [Test]
        public void DeserializeCache_WithExistingCacheAndChanges_CacheIsCorrect()
        {
            //Arrange
            List<FileSystemItem> cacheItems = new List<FileSystemItem>
                                             {
                                                 new FileSystemItem("name1", "folder1"),
                                                 new FileSystemItem("name1", "folder2"),
                                                 new FileSystemItem("C:\\")
                                             };
            FileSystemCache cache = new FileSystemCache(cacheItems, DateTime.Now.AddDays(-2));
            _serializer.SerializeCache(cache);

            List<FileSystemChangeEventArgs> changeItems = new List<FileSystemChangeEventArgs>
                                                              {
                                                                  new FileSystemChangeEventArgs("folder1", null),
                                                                  new FileSystemChangeEventArgs(null, "folder3"),
                                                                  new FileSystemChangeEventArgs("folder2", "folder4")
                                                              };

            DateTime dateTime = DateTime.Now;
            FileSystemChanges changes = new FileSystemChanges { Changes = changeItems, LastFullScanTime = dateTime };
            _serializer.SerializeCacheChanges(changes);

            //Act
            FileSystemCache actualCache = _serializer.DeserializeCache();

            //Assert
            //Serialization does not preserve milliseconds, so we use epsilon check.
            Assert.That(dateTime - actualCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 1)));
            Assert.That(actualCache.Items.Count, Is.EqualTo(3));

            Assert.That(actualCache.Items[0].FullPath, Is.EqualTo(Path.GetFullPath("C:\\")));
            Assert.That(actualCache.Items[0].Name, Is.EqualTo(""));

            Assert.That(actualCache.Items[1].FullPath, Is.EqualTo(Path.GetFullPath("folder3")));
            Assert.That(actualCache.Items[1].Name, Is.EqualTo("folder3"));

            Assert.That(actualCache.Items[2].FullPath, Is.EqualTo(Path.GetFullPath("folder4")));
            Assert.That(actualCache.Items[2].Name, Is.EqualTo("folder4"));
        }

        [Test]
        public void DeserializeCache_WithExistingCacheAndDoubleSavedChanges_CacheIsCorrect()
        {
            //Arrange
            List<FileSystemItem> cacheItems = new List<FileSystemItem>
                                             {
                                                 new FileSystemItem("name1", "folder1"),
                                                 new FileSystemItem("name1", "folder2"),
                                                 new FileSystemItem("C:\\")
                                             };
            FileSystemCache cache = new FileSystemCache(cacheItems, DateTime.Now.AddDays(-2));
            _serializer.SerializeCache(cache);

            List<FileSystemChangeEventArgs> changeItems = new List<FileSystemChangeEventArgs>
                                                              {
                                                                  new FileSystemChangeEventArgs("folder1", null),
                                                                  new FileSystemChangeEventArgs(null, "folder3"),
                                                              };

            FileSystemChanges changes = new FileSystemChanges { Changes = changeItems, LastFullScanTime = DateTime.Now.AddDays(-1) };
            _serializer.SerializeCacheChanges(changes);

            changeItems = new List<FileSystemChangeEventArgs>
                                                              {
                                                                  new FileSystemChangeEventArgs("folder2", "folder4")
                                                              };

            DateTime dateTime = DateTime.Now;
            changes = new FileSystemChanges { Changes = changeItems, LastFullScanTime = dateTime };
            _serializer.SerializeCacheChanges(changes);

            //Act
            FileSystemCache actualCache = _serializer.DeserializeCache();

            //Assert
            //Serialization does not preserve milliseconds, so we use epsilon check.
            Assert.That(dateTime - actualCache.LastFullScanTime, Is.LessThan(new TimeSpan(0, 0, 1)));
            Assert.That(actualCache.Items.Count, Is.EqualTo(3));

            Assert.That(actualCache.Items[0].FullPath, Is.EqualTo(Path.GetFullPath("C:\\")));
            Assert.That(actualCache.Items[0].Name, Is.EqualTo(""));

            Assert.That(actualCache.Items[1].FullPath, Is.EqualTo(Path.GetFullPath("folder3")));
            Assert.That(actualCache.Items[1].Name, Is.EqualTo("folder3"));

            Assert.That(actualCache.Items[2].FullPath, Is.EqualTo(Path.GetFullPath("folder4")));
            Assert.That(actualCache.Items[2].Name, Is.EqualTo("folder4"));
        }

        [Test]
        public void DeserializeCache_WithExistingChangesAndAbsentCache_CacheIsNull()
        {
            //Arrange
            List<FileSystemChangeEventArgs> changeItems = new List<FileSystemChangeEventArgs>
                                                              {
                                                                  new FileSystemChangeEventArgs("folder1", null),
                                                                  new FileSystemChangeEventArgs(null, "folder3"),
                                                                  new FileSystemChangeEventArgs("folder2", "folder4")
                                                              };
            DateTime dateTime = DateTime.Now;
            FileSystemChanges changes = new FileSystemChanges { Changes = changeItems, LastFullScanTime = DateTime.Now };
            _serializer.SerializeCacheChanges(changes);

            //Act
            FileSystemCache actualCache = _serializer.DeserializeCache();

            //Assert
            Assert.That(actualCache, Is.Null);
        }
    }
}
