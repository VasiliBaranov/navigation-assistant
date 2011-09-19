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
            _matches = new ObservableCollection<MatchModel>(GetMatchRepresentations(_searchText));

            PropertyChanged(this, new PropertyChangedEventArgs("Matches"));
            PropertyChanged(this, new PropertyChangedEventArgs("SearchText"));
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
        }

        public MatchModel SelectedMatch
        {
            get
            {
                return _selectedMatch;
            }
            set
            {
                _selectedMatch = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SelectedMatch"));
            }
        }

        #endregion

        #region Public Methods

        public void Navigate()
        {
            _navigationAssistant.NavigateTo(SelectedMatch.Path);
        }

        #endregion

        #region Non Public Methods

        private List<MatchModel> GetMatchRepresentations(string searchText)
        {
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
