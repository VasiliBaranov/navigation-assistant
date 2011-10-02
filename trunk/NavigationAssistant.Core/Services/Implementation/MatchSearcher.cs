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

            Regex matchRegex = GetMatchRegex(searchText);

            List<MatchedFileSystemItem> matches =
                items
                    .Select(i => GetMatchedItem(i, matchRegex))
                    .Where(mi => mi != null)
                    .ToList();

            return matches;
        }

        private MatchedFileSystemItem GetMatchedItem(FileSystemItem item, Regex matchRegex)
        {
            List<MatchSubstring> substrings = GetMatchSubstrings(item.Name, matchRegex);
            if (substrings == null)
            {
                return null;
            }

            MatchedFileSystemItem matchedItem = new MatchedFileSystemItem(item, new MatchString(substrings));

            return matchedItem;
        }

        private List<MatchSubstring> GetMatchSubstrings(string input, Regex matchRegex)
        {
            MatchCollection matches = matchRegex.Matches(input);
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

        private static Regex GetMatchRegex(string match)
        {
            //List<string> substrings = Utility.SplitStringByUpperChars(match);
            string[] substrings = match.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (ListUtility.IsNullOrEmpty(substrings))
            {
                return null;
            }

            string template = string.Join("[^\\s]*\\s+", substrings.ToArray());
            template = "\\b" + template; //Word boundary for the first word; for other words we require preceding spaces

            Regex matchRegex = new Regex(template, RegexOptions.IgnoreCase);

            return matchRegex;
        }
    }
}
