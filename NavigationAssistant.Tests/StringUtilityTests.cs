using System.Collections.Generic;
using NUnit.Framework;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Tests
{
    //Note: TestCase sources for different methods should not have equal names, 
    //as then when calling Method1 for Source1 a Method2 for the same Source1 will be called, and results will be overwritten.
    //That's really weird)
    [TestFixture]
    public class StringUtilityTests
    {
        [Test]
        [TestCaseSource("GetParseQuotedStringData")]
        public void ParseQuotedString_IsCorrect(string input, List<string> expectedSubstrings)
        {
            List<string> actualSubstrings = StringUtility.ParseQuotedString(input);

            Assert.That(actualSubstrings, Is.EquivalentTo(expectedSubstrings));
        }

        [Test]
        [TestCaseSource("GetBuildQuotedStringData")]
        public void BuildQuotedString_IsCorrect(List<string> substrings, string expectedOutput)
        {
            string actualOutput = StringUtility.BuildQuotedString(substrings);

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void MakeFirstLetterUppercase_IsCorrect()
        {
            
        }

        public IEnumerable<TestCaseData> GetParseQuotedStringData()
        {
            string input;
            List<string> expectedSubstrings;
            TestCaseData testCaseData;

            //Empty input
            input = string.Empty;
            expectedSubstrings = new List<string>();
            testCaseData = new TestCaseData(input, expectedSubstrings).SetName("Empty input");
            yield return testCaseData;

            //Null input
            input = null;
            expectedSubstrings = new List<string>();
            testCaseData = new TestCaseData(input, expectedSubstrings).SetName("Null input");
            yield return testCaseData;

            //Single string without quotes
            input = "asdasd";
            expectedSubstrings = new List<string> { "asdasd" };
            testCaseData = new TestCaseData(input, expectedSubstrings).SetName("Single string without quotes");
            yield return testCaseData;

            //Several strings without quotes
            input = "C:\\asd D:\\sdf";
            expectedSubstrings = new List<string> { "C:\\asd", "D:\\sdf" };
            testCaseData = new TestCaseData(input, expectedSubstrings).SetName("Several strings without quotes");
            yield return testCaseData;

            //Single quoted string
            input = "\"C:\\asd asd\"";
            expectedSubstrings = new List<string> { "C:\\asd asd" };
            testCaseData = new TestCaseData(input, expectedSubstrings).SetName("Single quoted string");
            yield return testCaseData;

            //Multiple quoted strings
            input = "\"C:\\asd asd\" \"D:\\sdf sdf\"";
            expectedSubstrings = new List<string> { "C:\\asd asd", "D:\\sdf sdf" };
            testCaseData = new TestCaseData(input, expectedSubstrings).SetName("Multiple quoted strings");
            yield return testCaseData;

            //Mixed quoting (first string quoted)
            input = "\"C:\\asd asd\"D:\\sdf \t \"   D:\\sdf sdf\"    \"D:\\sdf sdf   \"";
            expectedSubstrings = new List<string> { "C:\\asd asd", "D:\\sdf", "   D:\\sdf sdf", "D:\\sdf sdf   " };
            testCaseData = new TestCaseData(input, expectedSubstrings).SetName("Mixed quoting (first string quoted)");
            yield return testCaseData;

            //Mixed quoting (second string quoted)
            input = "  D:\\sdf\"C:\\asd asd\" \t  \" \t D:\\sdf sdf\"    D:\\sdf \t ";
            expectedSubstrings = new List<string> { "D:\\sdf", "C:\\asd asd", " \t D:\\sdf sdf", "D:\\sdf" };
            testCaseData = new TestCaseData(input, expectedSubstrings).SetName("Mixed quoting (second string quoted)");
            yield return testCaseData;
        }

        public IEnumerable<TestCaseData> GetBuildQuotedStringData()
        {
            List<string> substrings;
            string expectedOutput;
            TestCaseData testCaseData;

            //Empty input
            expectedOutput = string.Empty;
            substrings = new List<string>();
            testCaseData = new TestCaseData(substrings, expectedOutput).SetName("Build Empty input");
            yield return testCaseData;

            //Null input
            expectedOutput = string.Empty;
            substrings = null;
            testCaseData = new TestCaseData(substrings, expectedOutput).SetName("Build Null input");
            yield return testCaseData;

            //Single string without quotes
            expectedOutput = "asdasd";
            substrings = new List<string> { "asdasd" };
            testCaseData = new TestCaseData(substrings, expectedOutput).SetName("Build Single string without quotes");
            yield return testCaseData;

            //Several strings without quotes
            expectedOutput = "C:\\asd D:\\sdf";
            substrings = new List<string> { "C:\\asd", "D:\\sdf" };
            testCaseData = new TestCaseData(substrings, expectedOutput).SetName("Build Several strings without quotes");
            yield return testCaseData;

            //Single quoted string
            expectedOutput = "\"C:\\asd asd   \"";
            substrings = new List<string> { "C:\\asd asd   " };
            testCaseData = new TestCaseData(substrings, expectedOutput).SetName("Build Single quoted string");
            yield return testCaseData;

            //Multiple quoted strings
            expectedOutput = "\"C:\\asd asd\" \"D:\\sdf sdf\"";
            substrings = new List<string> { "C:\\asd asd", "D:\\sdf sdf" };
            testCaseData = new TestCaseData(substrings, expectedOutput).SetName("Build Multiple quoted strings");
            yield return testCaseData;

            //Mixed quoting (first string quoted)
            expectedOutput = "\"C:\\asd asd\" D:\\sdf \"   D:\\sdf sdf\" \"D:\\sdf sdf   \"";
            substrings = new List<string> { "C:\\asd asd", "D:\\sdf", "   D:\\sdf sdf", "D:\\sdf sdf   " };
            testCaseData = new TestCaseData(substrings, expectedOutput).SetName("Build Mixed quoting (first string quoted)");
            yield return testCaseData;

            //Mixed quoting (second string quoted)
            expectedOutput = "D:\\sdf \"C:\\asd asd\" \" \t D:\\sdf sdf\" D:\\sdf \"D:\\sdf\t\"";
            substrings = new List<string> { "D:\\sdf", "C:\\asd asd", " \t D:\\sdf sdf", "D:\\sdf", "D:\\sdf\t" };
            testCaseData = new TestCaseData(substrings, expectedOutput).SetName("Build Mixed quoting (second string quoted)");
            yield return testCaseData;
        }
    }
}
