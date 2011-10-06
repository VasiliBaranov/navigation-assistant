using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class MatchSearcher : IMatchSearcher
    {
        public List<MatchedFileSystemItem> GetMatches(List<FileSystemItem> items, string searchText)
        {
            if (ListUtility.IsNullOrEmpty(items) || string.IsNullOrEmpty(searchText))
            {
                return new List<MatchedFileSystemItem>();
            }

            Regex searchRegex = GetSearchRegex(searchText);

            List<MatchedFileSystemItem> matches =
                items
                    .Select(i => GetMatchedItem(i, searchRegex))
                    .Where(mi => mi != null)
                    .ToList();

            return matches;
        }

        private MatchedFileSystemItem GetMatchedItem(FileSystemItem item, Regex searchRegex)
        {
            List<MatchSubstring> substrings = GetMatchSubstrings(item.Name, searchRegex);
            if (substrings == null)
            {
                return null;
            }

            MatchedFileSystemItem matchedItem = new MatchedFileSystemItem(item, new MatchString(substrings));

            return matchedItem;
        }

        private static List<MatchSubstring> GetMatchSubstrings(string input, Regex searchRegex)
        {
            MatchCollection matches = searchRegex.Matches(input);
            List<MatchSubstring> substrings = new List<MatchSubstring>();

            bool hasSuccessfulMatches = matches.Cast<Match>().Any(m => m.Success);
            if (!hasSuccessfulMatches)
            {
                return null;
            }

            // Example : itemName = "{0}asd{1}sdf{2}{3}dfg"; regex is \{\d+\}
            int lastNonFormatIndex = 0;
            foreach (Match match in matches)
            {
                if (!match.Success)
                {
                    continue;
                }

                // This check is to handle "{0}" and "{2}{3}"
                if (match.Index > lastNonFormatIndex)
                {
                    // For the substring "asd" match.Index = 6, lastNonFormatIndex = 3
                    string notMatchedSubstring = input.Substring(lastNonFormatIndex, match.Index - lastNonFormatIndex);
                    substrings.Add(new MatchSubstring(notMatchedSubstring, false));
                }

                substrings.Add(new MatchSubstring(match.Value, true));

                lastNonFormatIndex = match.Index + match.Length;
            }

            //To handle "dfg"
            if (lastNonFormatIndex < input.Length)
            {
                string notMatchedSubstring = input.Substring(lastNonFormatIndex);
                substrings.Add(new MatchSubstring(notMatchedSubstring, false));
            }

            return substrings;
        }

        private static Regex GetSearchRegex(string searchText)
        {
            string[] substrings = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (ListUtility.IsNullOrEmpty(substrings))
            {
                return null;
            }

            substrings = substrings.Select(Regex.Escape).ToArray();

            string[] substringTemplates = substrings.Select(GetWordStartTemplate).ToArray();

            string template = string.Join(@"[^\s]*", substringTemplates);
            Regex searchRegex = new Regex(template);

            return searchRegex;
        }

        private static string GetWordStartTemplate(string wordStart, int wordIndex)
        {
            string firstLetterTemplate = GetFirstLetterTemplate(wordStart[0], wordIndex);

            string otherLettersTemplate = wordStart.Length > 1
                                              ? string.Format("(?i:{0})", wordStart.Substring(1))
                                              : string.Empty;

            return firstLetterTemplate + otherLettersTemplate;
        }

        private static string GetFirstLetterTemplate(char firstLetter, int wordIndex)
        {
            string template = wordIndex == 0 
                ? @"(?:\b[{0}{1}]|{1})" 
                : @"(?:\s+[{0}{1}]|{1})";

            return string.Format(template, char.ToLower(firstLetter), char.ToUpper(firstLetter));
        }
    }
}
