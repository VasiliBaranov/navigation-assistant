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
    }
}
