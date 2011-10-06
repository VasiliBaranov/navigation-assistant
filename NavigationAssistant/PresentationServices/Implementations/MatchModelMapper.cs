using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;
using NavigationAssistant.Properties;
using NavigationAssistant.ViewModel;

namespace NavigationAssistant.PresentationServices.Implementations
{
    public class MatchModelMapper : IMatchModelMapper
    {
        public List<MatchModel> GetMatchModels(List<MatchedFileSystemItem> folderMatches)
        {
            if (ListUtility.IsNullOrEmpty(folderMatches))
            {
                return new List<MatchModel> {new MatchModel(this, Resources.NoMatchesFound)};
            }

            folderMatches.Sort(Compare);
            List<MatchModel> matchRepresentations = folderMatches
                .Take(Constants.MaxMatchesToDisplay)
                .Select(GetMatchModel)
                .ToList();

            if (folderMatches.Count > Constants.MaxMatchesToDisplay)
            {
                matchRepresentations.Add(new MatchModel(this, Resources.TooManyMatchesText));
            }

            return matchRepresentations;
        }

        public MatchModel GetMatchModel(MatchedFileSystemItem match)
        {
            //Clone the matched item name
            MatchString matchedItemName = new MatchString(match.MatchedItemName);
            string path = string.Format(CultureInfo.InvariantCulture, " -> {0}", match.FullPath);
            matchedItemName.Add(new MatchSubstring(path, false));

            return new MatchModel(this, matchedItemName, match.FullPath);
        }

        public TextBlock GetTextBlock(MatchString matchedText, TextDecorationCollection matchDecoration, Brush matchColor)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;

            foreach (MatchSubstring matchSubstring in matchedText)
            {
                Run substringControl = new Run(matchSubstring.Value);
                if (matchSubstring.IsMatched)
                {
                    substringControl.TextDecorations = matchDecoration;
                    substringControl.Foreground = matchColor;
                }

                textBlock.Inlines.Add(substringControl);
            }

            return textBlock;
        }

        public int Compare(MatchedFileSystemItem x, MatchedFileSystemItem y)
        {
            int? validityResult = CompareValidity(x, y);
            if (validityResult != null)
            {
                return validityResult.Value;
            }

            bool xIsPerfectMatch = x.MatchedItemName.Count == 1;
            bool yIsPerfectMatch = y.MatchedItemName.Count == 1;

            if(xIsPerfectMatch && !yIsPerfectMatch)
            {
                return -1;
            }

            if (!xIsPerfectMatch && yIsPerfectMatch)
            {
                return 1;
            }

            if (x.FullPath.Length != y.FullPath.Length)
            {
                return x.FullPath.Length.CompareTo(y.FullPath.Length);
            }

            return string.Compare(x.FullPath, y.FullPath, StringComparison.CurrentCulture);
        }

        private static int? CompareValidity(MatchedFileSystemItem x, MatchedFileSystemItem y)
        {
            int? nullComparison = CompareByNull(x, y);
            if (nullComparison != null)
            {
                return nullComparison.Value;
            }

            int? matchNullComparison = CompareByNull(x.MatchedItemName, y.MatchedItemName);
            if (matchNullComparison != null)
            {
                return matchNullComparison.Value;
            }

            int? pathNullComparison = CompareByNull(x.FullPath, y.FullPath);
            if (pathNullComparison != null)
            {
                return pathNullComparison.Value;
            }

            return null;
        }

        private static int? CompareByNull(object x, object y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return 1;
            }

            if (x != null && y == null)
            {
                return -1;
            }

            return null;
        }
    }
}
