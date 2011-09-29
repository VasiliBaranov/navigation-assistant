using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Core.Utilities
{
    public static class StringUtility
    {
        public static List<string> SplitStringByUpperChars(string input)
        {
            List<string> result = new List<string>();

            if (String.IsNullOrEmpty(input))
            {
                return result;
            }

            int previousUpperCharIndex = 0;
            int inputLength = input.Length;

            for (int i = 0; i < inputLength; i++)
            {
                char currentChar = input[i];

                bool shouldAddSubstring = Char.IsUpper(currentChar) && (i != 0);

                if (!shouldAddSubstring)
                {
                    continue;
                }

                //E.g. input = myInput; previousUpperCharIndex = 0, i = 2.
                string substring = input.Substring(previousUpperCharIndex, i - previousUpperCharIndex);
                result.Add(substring);

                previousUpperCharIndex = i;
            }

            //Add the last substring
            string lastSubstring = input.Substring(previousUpperCharIndex);
            result.Add(lastSubstring);

            return result;
        }

        public static List<string> ParseQuotedString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<string>();
            }

            input = input.Trim();
            //let input='asd "sdf" "dfg" fgh ghj "hjk"'
            string[] substrings = input.Split('"');

            List<string> parsedStrings = new List<string>();

            int index = 0;
            foreach (string substring in substrings)
            {
                //Even if quote is the first symbol in the input, the first substring will be an empty one,
                //so the first quoted string will have an index == 1
                bool isQuotedSubstring = ((index - 1)% 2 == 0);

                SplitBySpace(substring, isQuotedSubstring, parsedStrings);

                index++;
            }

            return parsedStrings;
        }

        private static void SplitBySpace(string input, bool isQuoted, List<string> targetList)
        {
            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            string trimmedInput = input.Trim();
            if (string.IsNullOrEmpty(trimmedInput))
            {
                return;
            }

            if (isQuoted)
            {
                targetList.Add(input);
            }
            else
            {
                IEnumerable<string> noSpacesSubstrings = input
                    .Split(' ', '\t')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));

                targetList.AddRange(noSpacesSubstrings);
            }
        }

        public static string BuildQuotedString(List<string> values)
        {
            if (ListUtility.IsNullOrEmpty(values))
            {
                return string.Empty;
            }

            string[] quotedValues = values.Select(QuoteIfNecessary).ToArray();

            string result = string.Join(" ", quotedValues);

            return result;
        }

        private static string QuoteIfNecessary(string value)
        {
            if (value.Contains(" ") || value.Contains("\t"))
            {
                return string.Format(CultureInfo.InvariantCulture, "\"{0}\"", value);
            }

            return value;
        }
    }
}
