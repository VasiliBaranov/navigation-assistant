using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Threading;
using Core.Model;
using Core.Services;
using Core.Services.Implementation;
using System.Linq;
using Core.Utilities;

namespace WindowsExplorerClient
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly List<string> _rootFolders = new List<string> {"E:\\"};

        private readonly INavigationAssistant _navigationAssistant;

        private readonly DispatcherTimer _delayTimer;

        private ObservableCollection<MatchModel> _matches;

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
            //_navigationAssistant = new NavigationAssistant(fileSystemParser, new MatchSearcher(), new WindowsExplorerManager());
            _navigationAssistant = new NavigationAssistant(fileSystemParser, new MatchSearcher(), new TotalCommanderManager(@"d:\Program Files\Total Commander\TOTALCMD.EXE"));

            _delayTimer = new DispatcherTimer();
            _delayTimer.Interval = TimeSpan.FromMilliseconds(DelayInMilliseconds);
            _delayTimer.Tick += HandleDelayElapsed;

            _matches = new ObservableCollection<MatchModel> {GetMatchModel(InitialMatchesMessage)};
        }

        private void HandleDelayElapsed(object sender, EventArgs e)
        {
            Matches = new ObservableCollection<MatchModel>(GetMatchRepresentations(_searchText));
            SelectedMatch = Matches[0];
            _delayTimer.Stop(); //Would like to handle tick just once

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
            if (Utility.IsNullOrEmpty(Matches))
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
            if (Utility.IsNullOrEmpty(Matches))
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
                return new List<MatchModel> { GetMatchModel(InitialMatchesMessage) };
            }

            List<MatchedFileSystemItem> folderMatches = _navigationAssistant.GetFolderMatches(_rootFolders, searchText);
            if (Utility.IsNullOrEmpty(folderMatches))
            {
                return new List<MatchModel> { GetMatchModel(NoMatchesFound) };
            }

            List<MatchModel> matchRepresentations = folderMatches
                .Take(MaxMatchesToDisplay)
                .OrderBy(m=>m.ItemPath.Length)
                .Select(GetMatchRepresentation)
                .ToList();

            if (folderMatches.Count > MaxMatchesToDisplay)
            {
                matchRepresentations.Add(GetMatchModel(TooManyMatchesText));
            }

            return matchRepresentations;
        }

        private MatchModel GetMatchRepresentation(MatchedFileSystemItem match)
        {
            //Clone the matched item name
            MatchString matchedItemName = new MatchString(match.MatchedItemName);
            string path = string.Format(CultureInfo.InvariantCulture, " -> {0}", match.ItemPath);
            matchedItemName.Add(new MatchSubstring(path, false));

            return new MatchModel(matchedItemName, match.ItemPath);
        }

        private MatchModel GetMatchModel(string text)
        {
            MatchSubstring substring = new MatchSubstring(text, false);
            List<MatchSubstring> substrings = new List<MatchSubstring>{substring};

            return new MatchModel(new MatchString(substrings), null);
        }

        #endregion
    }
}
