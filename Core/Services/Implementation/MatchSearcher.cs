﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Model;

namespace Core.Services.Implementation
{
    public class MatchSearcher : IMatchSearcher
    {
        public List<MatchedFileSystemItem> GetMatches(List<FileSystemItem> items, string match)
        {
            if (Utilities.IsNullOrEmpty(items) || string.IsNullOrEmpty(match))
            {
                return new List<MatchedFileSystemItem>();
            }

            Regex matchRegex = GetMatchRegex(match);

            List<MatchedFileSystemItem> matches =
                items
                    .Where(i => matchRegex.IsMatch(i.ItemName))
                    .Select(i => new MatchedFileSystemItem(i))
                    .ToList();

            return matches;
        }

        private static Regex GetMatchRegex(string match)
        {
            List<string> substrings = Utilities.SplitStringByUpperChars(match);

            if (Utilities.IsNullOrEmpty(substrings))
            {
                return null;
            }

            string template = string.Join(".*", substrings.ToArray());
            template = string.Format(CultureInfo.InvariantCulture, ".*{0}.*", template);

            Regex matchRegex = new Regex(template, RegexOptions.IgnoreCase);

            return matchRegex;
        }
    }
}
