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

            yield return CreateTestCaseData("my doc", "y d", null, "Match just from word start (first word violation)");

            yield return CreateTestCaseData("my doc", "m d", "my d", "Simple match of two words");

            yield return CreateTestCaseData("my own doc", "m d", null, "Match just neighboring words");

            yield return CreateTestCaseData("my doc", "m o", null, "Match just from word start (second word violation)");

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

            yield return CreateTestCaseData("myDoc", "m dO", "myDo", "Match camel/pascal casing");

            yield return CreateTestCaseData("my    Doc", "m do", "my    Do", "Ignore multiple spaces in item name");

            yield return CreateTestCaseData("my Doc", "m   do", "my Do", "Ignore multiple spaces in search text");

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

            yield return CreateTestCaseData("mydoc", "m do", null, "Do not match words not separated with space or camel case");
        }

        public IEnumerable<TestCaseData> GetMatchCasesWithSpecialSymbols()
        {
            yield return CreateTestCaseData("mydoc", "[ ", null, "Correctly handle special symbol");

            yield return CreateTestCaseData("mydoc", "] [ ^ $ # @ ! ~ ' \" > < . , ? / \\ = - + _ ` ", null, "Correctly handle several special symbols");

            yield return CreateTestCaseData("mydoc", "] [^$# @ ~[' \"><. ,?/\\ =-+_]`", null, "Correctly handle invalid regex with grouped symbols");

            yield return CreateTestCaseData(".my doc", ".m d", ".my d", "Match word with special symbol at string start");

            yield return CreateTestCaseData("my doc .net", "m .n", null, "Special symbols with space do not match spaces before");

            yield return CreateTestCaseData("my.doc.doc", "m .d", "my.doc.d", "Special symbols with space match special symbols before");

            //Naming convention: "10_01" means that there is a space before the dot in the folder name
            //and a space after the dot in the search text.
            yield return CreateTestCaseData("my.doc", "m.d", null, "00_00");
            yield return CreateTestCaseData("my.doc", "m. d", null, "00_01");
            yield return CreateTestCaseData("my.doc", "m .d", "my.d", "00_10"); //Dots match any symbols before themselves, even space. Not sure if it's user-friendly
            yield return CreateTestCaseData("my.doc", "m . d", null, "00_11");
            yield return CreateTestCaseData("my.doc", "m d", "my.d", "00_noDot");
            yield return CreateTestCaseData("my.doc", "my.d", "my.d", "00_full");
            yield return CreateTestCaseData("my.doc", "my. d", null, "00_full 1"); //No space after dot in the search text-no match. Not sure if it's perfect

            yield return CreateTestCaseData("my .doc", "m.d", null, "10_00");
            yield return CreateTestCaseData("my .doc", "m. d", null, "10_01");
            yield return CreateTestCaseData("my .doc", "m .d", "my .d", "10_10");
            yield return CreateTestCaseData("my .doc", "m . d", null, "10_11");
            yield return CreateTestCaseData("my .doc", "m d", "my .d", "10_noDot");

            yield return CreateTestCaseData("my. doc", "m.d", null, "01_00");
            yield return CreateTestCaseData("my. doc", "m. d", null, "01_01");
            yield return CreateTestCaseData("my. doc", "m .d", null, "01_10"); //No space after dot in the search text-no match. Not sure if it's perfect
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
    }
}
