using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NavigationAssistant.Core.Model;
using NavigationAssistant.ViewModel;

namespace NavigationAssistant.PresentationServices
{
    /// <summary>
    /// Defines methods for mapping matched file system items into match models (view models).
    /// </summary>
    public interface IMatchModelMapper
    {
        List<MatchModel> GetMatchModels(List<MatchedFileSystemItem> folderMatches);

        MatchModel GetMatchModel(MatchedFileSystemItem match);

        TextBlock GetTextBlock(MatchString matchedText, TextDecorationCollection matchDecoration, Brush matchColor);

        int Compare(MatchedFileSystemItem x, MatchedFileSystemItem y);
    }
}
