using System.Collections.Generic;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services.Implementation;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class FileSystemFilterUtilityTests
    {
        [Test]
        [TestCaseSource("GetTestCases")]
        public void TestCorrectness(FileSystemItem item, List<string> rootFolders, List<string> excludeFolderTemplates,
            bool isCorrect)
        {
            List<FileSystemItem> filteredItems = FileSystemFilterUtility.FilterCache(new List<FileSystemItem> {item},
                                                                                     rootFolders,
                                                                                     excludeFolderTemplates);

            int expectedFilteredItemsCount = isCorrect ? 1 : 0;
            Assert.That(filteredItems.Count, Is.EqualTo(expectedFilteredItemsCount));
        }

        public IEnumerable<TestCaseData> GetTestCases()
        {
            FileSystemItem item;
            List<string> rootFolders;
            List<string> excludeFolderTemplates;
            TestCaseData testCaseData;

            item = new FileSystemItem(@"C:\my doc");
            rootFolders = null;
            excludeFolderTemplates = null;
            testCaseData = new TestCaseData(item, rootFolders, excludeFolderTemplates, true).SetName("Null constraints handled correctly.");
            yield return testCaseData;

            item = new FileSystemItem(@"C:\my doc");
            rootFolders = new List<string>{"C:"};
            excludeFolderTemplates = null;
            testCaseData = new TestCaseData(item, rootFolders, excludeFolderTemplates, true).SetName("Item with correct root returned.");
            yield return testCaseData;

            item = new FileSystemItem(@"C:\my doc");
            rootFolders = new List<string> { "D:\\" };
            excludeFolderTemplates = null;
            testCaseData = new TestCaseData(item, rootFolders, excludeFolderTemplates, false).SetName("Item with incorrect root not returned.");
            yield return testCaseData;

            item = new FileSystemItem(@"C:\my documents\temp");
            rootFolders = null;
            excludeFolderTemplates = new List<string>{"my doc.*"};
            testCaseData = new TestCaseData(item, rootFolders, excludeFolderTemplates, false).SetName("Item with excluded folder not returned.");
            yield return testCaseData;

            item = new FileSystemItem(@"C:\MY documents\temp");
            rootFolders = null;
            excludeFolderTemplates = new List<string> { "my doc.*" };
            testCaseData = new TestCaseData(item, rootFolders, excludeFolderTemplates, false).SetName("Item with excluded folder (with different case) not returned.");
            yield return testCaseData;

            item = new FileSystemItem(@"C:\documents\temp");
            rootFolders = null;
            excludeFolderTemplates = new List<string> { "my doc.*" };
            testCaseData = new TestCaseData(item, rootFolders, excludeFolderTemplates, true).SetName("Item no folders excluded is returned.");
            yield return testCaseData;
        }
    }
}
