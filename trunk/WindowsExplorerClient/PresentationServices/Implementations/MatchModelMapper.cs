using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Core.Model;
using Core.Services;
using Core.Utilities;
using WindowsExplorerClient.Properties;
using WindowsExplorerClient.ViewModel;

namespace WindowsExplorerClient.PresentationServices.Implementations
{
    public class MatchModelMapper : IMatchModelMapper
    {
        private readonly List<string> _rootFolders = new List<string> { "E:\\" };

        private const int MaxMatchesToDisplay = 20;

        private readonly INavigationAssistant _navigationAssistant;

        public MatchModelMapper(INavigationAssistant navigationAssistant)
        {
            _navigationAssistant = navigationAssistant;
        }

        public List<MatchModel> GetMatchModels(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return new List<MatchModel> { new MatchModel(this, Resources.InitialMatchesMessage) };
            }

            List<MatchedFileSystemItem> folderMatches = _navigationAssistant.GetFolderMatches(_rootFolders, searchText);
            if (Utility.IsNullOrEmpty(folderMatches))
            {
                return new List<MatchModel> { new MatchModel(this, Resources.NoMatchesFound) };
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
