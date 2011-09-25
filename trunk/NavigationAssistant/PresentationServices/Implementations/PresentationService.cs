using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Core.Utilities;
using NavigationAssistant.ViewModel;
using Application = System.Windows.Application;

namespace NavigationAssistant.PresentationServices.Implementations
{
    public class PresentationService : IPresentationService
    {
        public MatchModel MoveSelectionUp(ObservableCollection<MatchModel> matches, MatchModel selectedMatch)
        {
            if (Utility.IsNullOrEmpty(matches))
            {
                return null;
            }

            int selectionIndex = matches.IndexOf(selectedMatch);
            selectionIndex--;

            if (selectionIndex < 0)
            {
                selectionIndex = matches.Count - 1;
            }

            return matches[selectionIndex];
        }

        public MatchModel MoveSelectionDown(ObservableCollection<MatchModel> matches, MatchModel selectedMatch)
        {
            if (Utility.IsNullOrEmpty(matches))
            {
                return null;
            }

            int selectionIndex = matches.IndexOf(selectedMatch);
            selectionIndex++;

            if (selectionIndex == matches.Count)
            {
                selectionIndex = 0;
            }

            return matches[selectionIndex];
        }

        public double GetMaxMatchesListHeight(double searchTextBoxTop, double searchTextBoxHeight)
        {
            double windowTopPosition = CurrentWindow.Top;
            double screenHeight = ScreenWorkingArea.Height;

            double availableHeight = screenHeight - windowTopPosition - searchTextBoxTop - searchTextBoxHeight;
            return availableHeight * Constants.MaxScreenFillingRatio;
        }

        public double GetMaxMatchesListWidth(double searchTextBoxLeft)
        {
            double windowLeftPosition = CurrentWindow.Left;
            double screenWidth = ScreenWorkingArea.Width;

            double availableWidth = screenWidth - windowLeftPosition - searchTextBoxLeft;
            return availableWidth * Constants.MaxScreenFillingRatio;
        }

        private Rectangle ScreenWorkingArea
        {
            get
            {
                //Need to do some hacks to handle multiple monitors.
                //For a single monitor System.Windows.SystemParameters.WorkArea or System.Windows.SystemParameters.PrimaryScreenHeight
                //would have been sufficient (as they return primary monitor parameters).
                //See http://stackoverflow.com/questions/254197/how-can-i-get-the-active-screen-dimensions
                Screen screen = GetScreen(CurrentWindow);

                return screen.WorkingArea;
            }
        }

        private Window CurrentWindow
        {
            get { return Application.Current.MainWindow; }
        }

        private static Screen GetScreen(Window window)
        {
            return Screen.FromHandle(new WindowInteropHelper(window).Handle);
        }

    }
}
