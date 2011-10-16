using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using NavigationAssistant.ViewModel;

namespace NavigationAssistant.PresentationServices
{
    public interface IPresentationService
    {
        MatchModel MoveSelectionUp(ObservableCollection<MatchModel> matches, MatchModel selectedMatch);

        MatchModel MoveSelectionDown(ObservableCollection<MatchModel> matches, MatchModel selectedMatch);

        double GetMaxMatchesListHeight(double searchTextBoxTop, double searchTextBoxHeight);

        double GetMaxMatchesListWidth(double searchTextBoxLeft);

        bool ApplicationIsRunning(Mutex mutex);

        void MakeForeground(Window window);
    }
}
