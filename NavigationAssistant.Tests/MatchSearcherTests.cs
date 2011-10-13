using System;
using System.Collections.Generic;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class MatchSearcherTests
    {
        private IMatchSearcher _matchSearcher;

        [SetUp]
        public void SetUp()
        {
            _matchSearcher = new MatchSearcher();
        }

        [Test]
        [TestCaseSource("GetMatchCases")]
        public void GetMatches_WithoutSpecialSymbols_MatchesCorrect(FileSystemItem item, string searchText, MatchedFileSystemItem expectedMatch)
        {
            MatchesCorrect(item, searchText, expectedMatch);
        }

        [Test]
        [TestCaseSource("GetMatchCasesWithSpecialSymbols")]
        //[TestCaseSource("GetTempMatchCases")]
        public void GetMatches_WithSpecialSymbols_MatchesCorrect(FileSystemItem item, string searchText, MatchedFileSystemItem expectedMatch)
        {
            MatchesCorrect(item, searchText, expectedMatch);
        }

        private void MatchesCorrect(FileSystemItem item, string searchText, MatchedFileSystemItem expectedMatch)
        {
            List<FileSystemItem> items = new List<FileSystemItem> {item};

            List<MatchedFileSystemItem> actualMatches = _matchSearcher.GetMatches(items, searchText);

            bool matchExistenceCorrect = ((expectedMatch == null) && (ListUtility.IsNullOrEmpty(actualMatches))) ||
                                         ((expectedMatch != null) && (!ListUtility.IsNullOrEmpty(actualMatches)));

            Assert.That(matchExistenceCorrect, Is.True);

            if (expectedMatch == null)
            {
                return;
            }

            Assert.That(MatchesEqual(actualMatches[0], expectedMatch));
        }

        private static bool MatchesEqual(MatchedFileSystemItem actualMatch, MatchedFileSystemItem expectedMatch)
        {
            bool result =
                string.Equals(actualMatch.FullPath, expectedMatch.FullPath, StringComparison.Ordinal) &&
                string.Equals(actualMatch.Name, expectedMatch.Name, StringComparison.Ordinal) &&
                MatchedStringsEqual(actualMatch.MatchedItemName, expectedMatch.MatchedItemName);

            return result;
        }

        private static bool MatchedStringsEqual(MatchString actualString, MatchString expectedString)
        {
            if (actualString == null && expectedString == null)
            {
                return true;
            }

            if (!(actualString != null && expectedString != null))
            {
                return false;
            }

            if (actualString.Count != expectedString.Count)
            {
                return false;
            }

            for (int i = 0; i < actualString.Count; i++)
            {
                if (!MatchedSubtringsEqual(actualString[i], expectedString[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MatchedSubtringsEqual(MatchSubstring actualSubstring, MatchSubstring expectedSubstring)
        {
            if (actualSubstring == null && expectedSubstring == null)
            {
                return true;
            }

            if (!(actualSubstring != null && expectedSubstring != null))
            {
                return false;
            }

            return actualSubstring.IsMatched == expectedSubstring.IsMatched &&
                   string.Equals(actualSubstring.Value, expectedSubstring.Value, StringComparison.Ordinal);
        }

        public IEnumerable<TestCaseData> GetMatchCases()
        {
            const string rootPath = @"C:\";

            FileSystemItem item;
            string searchText;
            MatchString matchString;
            MatchedFileSystemItem expectedMatch;
            TestCaseData testCaseData;

            //1
            item = new FileSystemItem(rootPath + "my doc");
            searchText = "y d";
            expectedMatch = null;
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Match just from word start (first word violation)");
            yield return testCaseData;

            //2
            item = new FileSystemItem(rootPath + "my doc");
            searchText = "m d";
            matchString = new MatchString
                              {
                                  new MatchSubstring("my d", true),
                                  new MatchSubstring("oc", false)
                              };
            expectedMatch = new MatchedFileSystemItem(item, matchString);
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Simple match of two words");
            yield return testCaseData;

            //3
            item = new FileSystemItem(rootPath + "my own doc");
            searchText = "m d";
            expectedMatch = null;
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Match just neighboring words");
            yield return testCaseData;

            //4
            item = new FileSystemItem(rootPath + "my doc");
            searchText = "m o";
            expectedMatch = null;
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Match just from word start (second word violation)");
            yield return testCaseData;

            //5
            item = new FileSystemItem(rootPath + "my own doc");
            searchText = "ow Do";
            matchString = new MatchString
                              {
                                  new MatchSubstring("my ", false),
                                  new MatchSubstring("own do", true),
                                  new MatchSubstring("c", false),
                              };
            expectedMatch = new MatchedFileSystemItem(item, matchString);
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Simple match from the second word");
            yield return testCaseData;

            //6
            item = new FileSystemItem(rootPath + "my oWn dOc");
            searchText = "Ow Do";
            matchString = new MatchString
                              {
                                  new MatchSubstring("my ", false),
                                  new MatchSubstring("oWn dO", true),
                                  new MatchSubstring("c", false),
                              };
            expectedMatch = new MatchedFileSystemItem(item, matchString);
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Simple match from the second word (ignored case)");
            yield return testCaseData;

            //7
            item = new FileSystemItem(rootPath + "myDoc");
            searchText = "m dO";
            matchString = new MatchString
                              {
                                  new MatchSubstring("myDo", true),
                                  new MatchSubstring("c", false)
                              };
            expectedMatch = new MatchedFileSystemItem(item, matchString);
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Match camel/pascal casing");
            yield return testCaseData;

            //8
            item = new FileSystemItem(rootPath + "my    Doc");
            searchText = "m do";
            matchString = new MatchString
                              {
                                  new MatchSubstring("my    Do", true),
                                  new MatchSubstring("c", false)
                              };
            expectedMatch = new MatchedFileSystemItem(item, matchString);
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Ignore multiple spaces in item name");
            yield return testCaseData;

            //9
            item = new FileSystemItem(rootPath + "my Doc");
            searchText = "m   do";
            matchString = new MatchString
                              {
                                  new MatchSubstring("my Do", true),
                                  new MatchSubstring("c", false)
                              };
            expectedMatch = new MatchedFileSystemItem(item, matchString);
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Ignore multiple spaces in search text");
            yield return testCaseData;

            //10
            item = new FileSystemItem(rootPath + "myOwnDoc");
            searchText = "o do";
            matchString = new MatchString
                              {
                                  new MatchSubstring("my", false),
                                  new MatchSubstring("OwnDo", true),
                                  new MatchSubstring("c", false)
                              };
            expectedMatch = new MatchedFileSystemItem(item, matchString);
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Match camel/pascal casing in the middle");
            yield return testCaseData;

            //10
            item = new FileSystemItem(rootPath + "mydoc");
            searchText = "m do";
            expectedMatch = null;
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Do not match words not separated with space or camel case");
            yield return testCaseData;
        }

        public IEnumerable<TestCaseData> GetMatchCasesWithSpecialSymbols()
        {
            const string rootPath = @"C:\";

            FileSystemItem item;
            string searchText;
            MatchString matchString;
            MatchedFileSystemItem expectedMatch;
            TestCaseData testCaseData;

            item = new FileSystemItem(rootPath + "mydoc");
            searchText = "[ ";
            expectedMatch = null;
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Correctly handle special symbol");
            yield return testCaseData;

            item = new FileSystemItem(rootPath + "mydoc");
            searchText = "] [ ^ $ # @ ! ~ ' \" > < . , ? / \\ = - + _ ` ";
            expectedMatch = null;
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Correctly handle several special symbols");
            yield return testCaseData;

            item = new FileSystemItem(rootPath + "mydoc");
            searchText = "] [^$# @ ~[' \"><. ,?/\\ =-+_]`";
            expectedMatch = null;
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Correctly handle invalid regex with grouped symbols");
            yield return testCaseData;

            item = new FileSystemItem(rootPath + ".my doc");
            searchText = ".m d";
            matchString = new MatchString
                              {
                                  new MatchSubstring(".my d", true),
                                  new MatchSubstring("oc", false),
                              };
            expectedMatch = new MatchedFileSystemItem(item, matchString);
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Match word with special symbol at string start");
            yield return testCaseData;

            //Naming convention: "10_01" means that there is a space before the dot in the folder name
            //and a space after the dot in the search text.
            yield return CreateTestCaseData("my.doc", "m.d", null, "00_00");
            yield return CreateTestCaseData("my.doc", "m. d", null, "00_01");
            yield return CreateTestCaseData("my.doc", "m .d", null, "00_10");
            yield return CreateTestCaseData("my.doc", "m . d", null, "00_11");
            yield return CreateTestCaseData("my.doc", "m d", "my.d", "00_noDot");
            yield return CreateTestCaseData("my.doc", "my.d", "my.d", "00_full");
            yield return CreateTestCaseData("my.doc", "my. d", "my.d", "00_full 1");

            yield return CreateTestCaseData("my .doc", "m.d", null, "10_00");
            yield return CreateTestCaseData("my .doc", "m. d", null, "10_01");
            yield return CreateTestCaseData("my .doc", "m .d", "my .d", "10_10");
            yield return CreateTestCaseData("my .doc", "m . d", null, "10_11");
            yield return CreateTestCaseData("my .doc", "m d", "my .d", "10_noDot");

            yield return CreateTestCaseData("my. doc", "m.d", null, "01_00");
            yield return CreateTestCaseData("my. doc", "m. d", null, "01_01");
            yield return CreateTestCaseData("my. doc", "m .d", "my. d", "01_10");
            yield return CreateTestCaseData("my. doc", "m . d", "my. d", "01_11");
            yield return CreateTestCaseData("my. doc", "m d", "my. d", "01_noDot");

            yield return CreateTestCaseData("my . doc", "m.d", null, "11_00");
            yield return CreateTestCaseData("my . doc", "m. d", null, "11_01");
            yield return CreateTestCaseData("my . doc", "m .d", null, "11_10");
            yield return CreateTestCaseData("my . doc", "m . d", "my . d", "11_11");
            yield return CreateTestCaseData("my . doc", "m d", "my . d", "11_noDot");
        }

        private TestCaseData CreateTestCaseData(string folderName, string searchText, string matchedSubstring, string testName)
        {
            FileSystemItem item = new FileSystemItem("C:\\" + folderName);
            MatchedFileSystemItem expectedMatch;
            if (string.IsNullOrEmpty(matchedSubstring))
            {
                expectedMatch = null;
            }
            else
            {
                MatchString matchString = new MatchString
                                  {
                                      new MatchSubstring(matchedSubstring, true),
                                      new MatchSubstring(folderName.Substring(matchedSubstring.Length), false),
                                  };
                expectedMatch = new MatchedFileSystemItem(item, matchString);
            }
            
            TestCaseData testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName(testName);
            return testCaseData;
        }

        public IEnumerable<TestCaseData> GetTempMatchCases()
        {
            const string rootPath = @"C:\";

            FileSystemItem item;
            string searchText;
            MatchString matchString;
            MatchedFileSystemItem expectedMatch;
            TestCaseData testCaseData;

            yield return CreateTestCaseData("my.doc", "m .d", null, "00_10");
        }
    }
}
