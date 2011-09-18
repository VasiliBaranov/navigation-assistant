using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Core.Model;
using Core.Services;
using Core.Services.Implementation;
using System.Linq;

namespace WindowsExplorerClient
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly List<string> _rootFolders = new List<string> {"E:\\"};

        private readonly INavigationAssistant _navigationAssistant;

        private ObservableCollection<string> _matches = new ObservableCollection<string>();

        private string _searchText;

        public event PropertyChangedEventHandler PropertyChanged;

        private const int MaxMatchesToDisplay = 20;

        private const string TooManyMatchesText = "Too many matches...";

        #endregion

        #region Constructors

        public ViewModel()
        {
            IFileSystemParser fileSystemParser = new CachedFileSystemParser(new FileSystemParser(), new CacheSerializer(@"e:\temp\Cache.txt"));
            _navigationAssistant = new NavigationAssistant(fileSystemParser, new MatchSearcher());
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

                _matches = new ObservableCollection<string>(GetMatchRepresentations(_searchText));

                PropertyChanged(this, new PropertyChangedEventArgs("Matches"));
                PropertyChanged(this, new PropertyChangedEventArgs("SearchText"));
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
