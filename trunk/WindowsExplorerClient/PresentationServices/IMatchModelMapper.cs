using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core.Model;
using WindowsExplorerClient.ViewModel;

namespace WindowsExplorerClient.PresentationServices
{
    public interface IMatchModelMapper
    {
        List<MatchModel> GetMatchModels(string searchText);

        MatchModel GetMatchModel(MatchedFileSystemItem match);

        TextBlock GetTextBlock(MatchString matchedText, TextDecorationCollection matchDecoration, Brush matchColor);
    }
}
