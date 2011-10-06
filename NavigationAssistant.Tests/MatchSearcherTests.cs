using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public void GetMatches_MatchesCorrect(FileSystemItem item, string searchText, MatchedFileSystemItem expectedMatch)
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
            testCaseData = new TestCaseData(item, searchText, expectedMatch).SetName("Ignore multiple spaces");
            yield return testCaseData;
        }
    }
}
