using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Core;
using Core.Model;
using Core.Services;
using Core.Services.Implementation;
using System.Linq;
using Timer = System.Timers.Timer;

namespace WindowsExplorerClient
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly List<string> _rootFolders = new List<string> {"E:\\"};

        private readonly INavigationAssistant _navigationAssistant;

        private readonly Timer _delayTimer;

        private ObservableCollection<MatchModel> _matches = new ObservableCollection<MatchModel> { new MatchModel(InitialMatchesMessage, null) };

        private string _searchText;

        private MatchModel _selectedMatch;

        public event PropertyChangedEventHandler PropertyChanged;

        private const int DelayInMilliseconds = 200;

        private const int MaxMatchesToDisplay = 20;

        private const string TooManyMatchesText = "Too many matches...";

        private const string NoMatchesFound = "No matches found";

        private const string InitialMatchesMessage = "Please type folder name";

        #endregion

        #region Constructors

        public ViewModel()
        {
            IFileSystemParser fileSystemParser = new CachedFileSystemParser(new FileSystemParser(), new CacheSerializer(@"e:\temp\Cache.txt"));
            _navigationAssistant = new NavigationAssistant(fileSystemParser, new MatchSearcher());

            _delayTimer = new Timer();
            _delayTimer.AutoReset = false;
            _delayTimer.Interval = DelayInMilliseconds;
            _delayTimer.Elapsed += HandleDelayElapsed;
        }

        private void HandleDelayElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Matches = new ObservableCollection<MatchModel>(GetMatchRepresentations(_searchText));
            SelectedMatch = Matches[0];

            OnPropertyChanged("SearchText");
        }

        #endregion

        #region Properties

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;

                //To avoid too frequent updates we run a timer to take a pause.
                //If search text is not updated during this pause, matches will be rendered.

                //Need to stop the execution from the previous setter.
                _delayTimer.Stop();
                _delayTimer.Start();
            }
        }

        public ObservableCollection<MatchModel> Matches
        {
            get
            {
                return _matches;
            }
            set
            {
                _matches = value;
                OnPropertyChanged("Matches");
            }
        }

        public MatchModel SelectedMatch
        {
            get
            {
                return _selectedMatch;
            }
            set
            {
                if (_selectedMatch != null)
                {
                    _selectedMatch.IsFocused = false;
                }

                _selectedMatch = value;

                if (_selectedMatch != null)
                {
                    _selectedMatch.IsFocused = true;
                }

                OnPropertyChanged("SelectedMatch");
            }
        }

        public ApplicationWindow HostWindow { get; set; }

        #endregion

        #region Public Methods

        public bool CanNavigate()
        {
            return !string.IsNullOrEmpty(SelectedMatch.Path) && HostWindow != null;
        }

        public void Navigate()
        {
            _navigationAssistant.NavigateTo(SelectedMatch.Path, HostWindow);
        }

        public void MoveSelectionUp()
        {
            if (Utilities.IsNullOrEmpty(Matches))
            {
                return;
            }

            int selectionIndex = Matches.IndexOf(SelectedMatch);
            selectionIndex--;

            if (selectionIndex < 0)
            {
                selectionIndex = Matches.Count - 1;
            }

            SelectedMatch = Matches[selectionIndex];
        }

        public void MoveSelectionDown()
        {
            if (Utilities.IsNullOrEmpty(Matches))
            {
                return;
            }

            int selectionIndex = Matches.IndexOf(SelectedMatch);
            selectionIndex++;

            if (selectionIndex == Matches.Count)
            {
                selectionIndex = 0;
            }

            SelectedMatch = Matches[selectionIndex];
        }

        public void UpdateHostWindow()
        {
            HostWindow = _navigationAssistant.GetActiveWindow();
        }

        #endregion

        #region Non Public Methods

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private List<MatchModel> GetMatchRepresentations(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return new List<MatchModel> { new MatchModel(InitialMatchesMessage, null) };
            }

            List<MatchedFileSystemItem> folderMatches = _navigationAssistant.GetFolderMatches(_rootFolders, searchText);
            if (Utilities.IsNullOrEmpty(folderMatches))
            {
                return new List<MatchModel> { new MatchModel(NoMatchesFound, null) };
            }

            List<MatchModel> matchRepresentations = folderMatches
                .Take(MaxMatchesToDisplay)
                .OrderBy(m=>m.ItemPath.Length)
                .Select(GetMatchRepresentation)
                .ToList();

            if(folderMatches.Count > MaxMatchesToDisplay)
            {
                matchRepresentations.Add(new MatchModel(TooManyMatchesText, null));
            }

            return matchRepresentations;
        }

        private MatchModel GetMatchRepresentation(MatchedFileSystemItem match)
        {
            string text = string.Format(CultureInfo.InvariantCulture, "{0} -> {1}", match.ItemName, match.ItemPath);

            return new MatchModel(text, match.ItemPath);
        }

        #endregion
    }
}
