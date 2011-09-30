using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.Services;
using NavigationAssistant.PresentationModel;
using NavigationAssistant.ViewModel;

namespace NavigationAssistant.PresentationServices
{
    public interface IPresentationService
    {
        MatchModel MoveSelectionUp(ObservableCollection<MatchModel> matches, MatchModel selectedMatch);

        MatchModel MoveSelectionDown(ObservableCollection<MatchModel> matches, MatchModel selectedMatch);

        double GetMaxMatchesListHeight(double searchTextBoxTop, double searchTextBoxHeight);

        double GetMaxMatchesListWidth(double searchTextBoxLeft);

        INavigationService BuildNavigationService(Settings settings);

        bool ApplicationIsRunning();
    }
}
