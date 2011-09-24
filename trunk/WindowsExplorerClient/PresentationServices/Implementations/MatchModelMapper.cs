using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Core.Model;
using Core.Utilities;
using WindowsExplorerClient.Properties;
using WindowsExplorerClient.ViewModel;

namespace WindowsExplorerClient.PresentationServices.Implementations
{
    public class MatchModelMapper : IMatchModelMapper
    {
        private const int MaxMatchesToDisplay = 20;

        public List<MatchModel> GetMatchModels(List<MatchedFileSystemItem> folderMatches)
        {
            if (Utility.IsNullOrEmpty(folderMatches))
            {
                return new List<MatchModel> {new MatchModel(this, Resources.NoMatchesFound)};
            }

            List<MatchModel> matchRepresentations = folderMatches
                .Take(MaxMatchesToDisplay)
                .OrderBy(m => m.ItemPath.Length)
                .Select(GetMatchModel)
                .ToList();

            if (folderMatches.Count > MaxMatchesToDisplay)
            {
                matchRepresentations.Add(new MatchModel(this, Resources.TooManyMatchesText));
            }

            return matchRepresentations;
        }

        public MatchModel GetMatchModel(MatchedFileSystemItem match)
        {
            //Clone the matched item name
            MatchString matchedItemName = new MatchString(match.MatchedItemName);
            string path = string.Format(CultureInfo.InvariantCulture, " -> {0}", match.ItemPath);
            matchedItemName.Add(new MatchSubstring(path, false));

            return new MatchModel(this, matchedItemName, match.ItemPath);
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
    }
}
