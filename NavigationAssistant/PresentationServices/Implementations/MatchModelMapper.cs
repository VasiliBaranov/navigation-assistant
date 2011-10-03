﻿using System.Collections.Generic;
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

            List<MatchModel> matchRepresentations = folderMatches
                .OrderBy(m => m.FullPath.Length + m.FullPath)
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
    }
}
