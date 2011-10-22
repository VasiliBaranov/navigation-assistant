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
        #region Public Methods

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

        #endregion

        #region Non Public Methods

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
            IList<string> substrings = searchText.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (ListUtility.IsNullOrEmpty(substrings))
            {
                return null;
            }

            string[] substringTemplates = substrings.Select(GetWordStartTemplate).ToArray();

            //All meaningful substrings may be separated with any amount of non-space characters (as we split by space),
            //then followed by spaces (spaces will be specified in substringTemplates).
            string template = string.Join(@"\S*", substringTemplates);
            Regex searchRegex = new Regex(template);

            return searchRegex;
        }

        private static string GetWordStartTemplate(string wordStart, int wordIndex)
        {
            string firstLetterTemplate = GetFirstLetterTemplate(wordStart[0], wordIndex);

            //(?i ) means "ignore case"
            string otherLettersTemplate = wordStart.Length > 1
                                              ? string.Format("(?i:{0})", Regex.Escape(wordStart.Substring(1)))
                                              : string.Empty;

            return firstLetterTemplate + otherLettersTemplate;
        }

        private static string GetFirstLetterTemplate(char firstLetter, int wordIndex)
        {
            if (!char.IsLetter(firstLetter))
            {
                //Can not use \b, as \b doesn't work in conjunction with non-letter characters.
                //So we allow any amount of spaces before non-letter characters.
                //Non-space characters will be allowed when joining substring templates.
                //Also, there may be no preceding spaces, as non-letters should act as word boundaries.
                return "\\s*" + Regex.Escape(firstLetter.ToString());
            }

            //(?: ) means a non-capturing group.
            //These templates allow 
            //1. either an upper character after any character (space or non-space), so that "d" is matched in "MyDoc".
            //2. or lower and upper character after any non-letter character (\W+) (or at the word start, \b, for the first substring).
            string template = wordIndex == 0 
                                  ? @"(?:\b[{0}{1}]|{1})"
                                  : @"(?:\W+[{0}{1}]|{1})";

            return string.Format(template, char.ToLower(firstLetter), char.ToUpper(firstLetter));
        }

        #endregion
    }
}
