using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Model;
using NavigationAssistant.ViewModel;

namespace NavigationAssistant.PresentationServices
{
    public interface IMatchModelMapper
    {
        List<MatchModel> GetMatchModels(List<MatchedFileSystemItem> folderMatches);

        MatchModel GetMatchModel(MatchedFileSystemItem match);

        TextBlock GetTextBlock(MatchString matchedText, TextDecorationCollection matchDecoration, Brush matchColor);
    }
}
