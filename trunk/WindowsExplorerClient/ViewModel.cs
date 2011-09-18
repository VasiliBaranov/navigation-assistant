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

        private ObservableCollection<string> _matches = new ObservableCollection<string> { InitialMatchesMessage };

        private string _searchText;

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
            _matches = new ObservableCollection<string>(GetMatchRepresentations(_searchText));

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

        public ObservableCollection<string> Matches
        {
            get
            {
                return _matches;
            }
        }

        #endregion

        #region Non Public Methods

        private List<string> GetMatchRepresentations(string searchText)
        {
            List<MatchedFileSystemItem> folderMatches = _navigationAssistant.GetFolderMatches(_rootFolders, searchText);
            if (Utilities.IsNullOrEmpty(folderMatches))
            {
                return new List<string>{NoMatchesFound};
            }

            List<string> matchRepresentations = folderMatches
                .Take(MaxMatchesToDisplay)
                .OrderBy(m=>m.ItemPath.Length)
                .Select(GetMatchRepresentation)
                .ToList();

            if(folderMatches.Count > MaxMatchesToDisplay)
            {
                matchRepresentations.Add(TooManyMatchesText);
            }

            return matchRepresentations;
        }

        private string GetMatchRepresentation(MatchedFileSystemItem match)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} -> {1}", match.ItemName, match.ItemPath);
        }

        #endregion
    }
}
