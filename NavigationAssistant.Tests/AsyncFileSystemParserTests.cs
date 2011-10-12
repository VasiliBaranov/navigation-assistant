using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class AsyncFileSystemParserTests
    {
        private IAsyncFileSystemParser _asyncFileSystemParser;
        private const string FolderName = "Folder";
        private int _delayIntervalInSeconds;

        private FileSystemCache _parsedResult;
        private DateTime _parsingFinish;
        private readonly object _parsingSync = new object();

        [SetUp]
        public void SetUp()
        {
            _delayIntervalInSeconds = 0;
            _parsedResult = null;

            DirectoryUtility.EnsureClearFolder(FolderName);
        }

        [TearDown]
        public void TearDown()
        {
            _asyncFileSystemParser.Dispose();
            Directory.Delete(FolderName, true);
        }

        private void CreateParser()
        {
            FileSystemParser fileSystemParser =
                new FileSystemParser(new FileSystemListener(), new List<string> {FolderName});
            _asyncFileSystemParser = new AsyncFileSystemParser(fileSystemParser, _delayIntervalInSeconds);
            _asyncFileSystemParser.ParsingFinished += HandleParsingFinished;
        }

        private void HandleParsingFinished(object sender, ItemEventArgs<FileSystemCache> e)
        {
            lock (_parsingSync)
            {
                _parsedResult = e.Item;
                _parsingFinish = DateTime.Now;
            }
        }

        private void WaitForParsingFinish()
        {
            bool parsingFinished = false;
            while (!parsingFinished)
            {
                Thread.Sleep(100);

                lock (_parsingSync)
                {
                    parsingFinished = _parsedResult != null;
                }
            }
        }

        [Test]
        public void BeginParsing_WithDelay_ParsingStartedWithDelay()
        {
            _delayIntervalInSeconds = 2;
            CreateParser();

            //Need to start another thread, as far as the current thread will never be free to handle Timer event
            DateTime parsingStart = DateTime.Now;
            _asyncFileSystemParser.BeginParsing();

            WaitForParsingFinish();

            TimeSpan parsingTime = _parsingFinish - parsingStart;
            Assert.That(parsingTime.TotalSeconds, Is.GreaterThan(_delayIntervalInSeconds));
            Assert.That(parsingTime.TotalSeconds, Is.LessThan(_delayIntervalInSeconds + 1));
        }

        [Test]
        public void BeginParsing_WithoutDelay_ParsingStartedWithDelay()
        {
            _delayIntervalInSeconds = 0;
            CreateParser();

            DateTime parsingStart = DateTime.Now;
            _asyncFileSystemParser.BeginParsing();

            WaitForParsingFinish();

            TimeSpan parsingTime = _parsingFinish - parsingStart;
            Assert.That(parsingTime.TotalSeconds, Is.LessThan(1));
        }

        [Test]
        public void BeginParsing_WithoutDelay_ParsingResultsCorrect()
        {
            _delayIntervalInSeconds = 0;
            CreateParser();
            _asyncFileSystemParser.BeginParsing();

            WaitForParsingFinish();

            Assert.That(_parsedResult.Items.Select(i => i.Name).ToList(), Is.EquivalentTo(new List<string> {FolderName}));
            Assert.That((_parsingFinish - _parsedResult.LastFullScanTime).TotalSeconds, Is.LessThan(1));
        }
    }
}
