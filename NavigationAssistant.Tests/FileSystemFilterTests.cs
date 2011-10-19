using System.Collections.Generic;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class FileSystemFilterTests
    {
        private IFileSystemFilter _fileSystemFilter;

        [SetUp]
        public void SetUp()
        {
            _fileSystemFilter = new FileSystemFilter();
        }

        [Test]
        [TestCaseSource("GetTestCases")]
        public void TestCorrectness(FileSystemItem item, List<string> rootFolders, List<string> excludeFolderTemplates, bool isCorrect)
        {
            _fileSystemFilter.FoldersToParse = rootFolders;
            _fileSystemFilter.ExcludeFolderTemplates = excludeFolderTemplates;

            List<FileSystemItem> filteredItems = _fileSystemFilter.FilterItems(new List<FileSystemItem> { item });

            int expectedFilteredItemsCount = isCorrect ? 1 : 0;
            Assert.That(filteredItems.Count, Is.EqualTo(expectedFilteredItemsCount));
        }

        public IEnumerable<TestCaseData> GetTestCases()
        {
            //yield return CreateExcludeTemplateData(@"C:\my doc", null, true, "Null constraints handled correctly.");
            //yield return CreateExcludeTemplateData(@"C:\my documents\temp", "my doc.*", false, "Item with excluded folder not returned.");
            //yield return CreateExcludeTemplateData(@"C:\my documents\temp", "my doc", true, "Item with excluded folder not matched completely is returned.");
            //yield return CreateExcludeTemplateData(@"C:\MY documents\temp", "my doc.*", false, "Item with excluded folder (with different case) not returned.");
            //yield return CreateExcludeTemplateData(@"C:\documents\temp", "my doc.*", true, "Item with no folders excluded is returned.");
            yield return CreateExcludeTemplateData(@"C:\documents\.svn\temp", ".svn", false, "Svn folder excluded.");
            yield return CreateExcludeTemplateData(@"C:\documents\.svn\temp", "\\.svn", false, "Svn folder with escaped dot excluded.");

            yield return CreateRootFolderData(@"C:\my doc", "C:", true, "Item with correct root (root has no slash) returned.");
            yield return CreateRootFolderData(@"C:\my doc", "C:\\", true, "Item with correct root (root has a slash) returned.");
            yield return CreateRootFolderData(@"C:\my doc", "D:\\", false, "Item with incorrect root not returned.");
            yield return CreateRootFolderData(@"C:\my doc", "C:\\my", false, "Item with root, correct incompletely, not returned.");
            yield return CreateRootFolderData(@"C:\my doc\\", "C:\\my doc", true, "Item with correct root, but with slashes, returned.");
            yield return CreateRootFolderData(@"C:\my doc", "C:\\my doc\\", true, "Item with correct root, but without slashes, returned.");
            yield return CreateRootFolderData(@"c:\my doc", "C:\\my doc\\", true, "Item with correct root (but the first letter is lower) returned.");
            yield return CreateRootFolderData(@"C:\my doc", "c:\\my doc\\", true, "Item with correct root (but the first letter is upper) returned.");
            yield return CreateRootFolderData(@"C:\my doc\\", "C:\\my doc\\", true, "Item with correct root (when root and item have slashes) returned.");

        }

        private TestCaseData CreateExcludeTemplateData(string path, string excludeFolderTemplate, bool isMatched, string name)
        {
            FileSystemItem item = new FileSystemItem(path);
            List<string> excludeFolderTemplates = excludeFolderTemplate == null ? null : new List<string> { excludeFolderTemplate };
            TestCaseData testCaseData = new TestCaseData(item, null, excludeFolderTemplates, isMatched).SetName(name);
            return testCaseData;
        }

        private TestCaseData CreateRootFolderData(string path, string rootFolder, bool isMatched, string name)
        {
            FileSystemItem item = new FileSystemItem(path);
            List<string> rootFolders = new List<string> { rootFolder };
            TestCaseData testCaseData = new TestCaseData(item, rootFolders, null, isMatched).SetName(name);
            return testCaseData;
        }
    }
}
