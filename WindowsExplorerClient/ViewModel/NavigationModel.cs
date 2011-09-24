using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using Core.Model;
using Core.Services;
using Core.Services.Implementation;
using Core.Utilities;
using WindowsExplorerClient.PresentationServices;
using WindowsExplorerClient.PresentationServices.Implementations;
using WindowsExplorerClient.Properties;

namespace WindowsExplorerClient.ViewModel
{
    public class NavigationModel : INotifyPropertyChanged
    {
        #region Supplemetary Fields

        private readonly INavigationAssistant _navigationAssistant;

        private readonly IMatchModelMapper _matchModelMapper;

        private readonly IPresentationService _presentationService;

        private readonly DispatcherTimer _delayTimer;

        public event PropertyChangedEventHandler PropertyChanged;

        private const int DelayInMilliseconds = 200;

        private readonly List<string> _rootFolders = new List<string> { "E:\\" };

        #endregion

        #region Data Fields

        private ObservableCollection<MatchModel> _matches;

        private string _searchText;

        private MatchModel _selectedMatch;

        private double _searchTextBoxHeight;

        #endregion

        #region Constructors

        public NavigationModel()
        {
            IFileSystemParser fileSystemParser = new CachedFileSystemParser(new FileSystemParser(), new CacheSerializer(@"e:\temp\Cache.txt"));
            //_navigationAssistant = new NavigationAssistant(fileSystemParser, new MatchSearcher(), new WindowsExplorerManager());
            _navigationAssistant = new NavigationAssistant(fileSystemParser, new MatchSearcher(), new TotalCommanderManager(@"d:\Program Files\Total Commander\TOTALCMD.EXE"));

            _matchModelMapper = new MatchModelMapper();

            _presentationService = new PresentationService();

            _delayTimer = new DispatcherTimer();
            _delayTimer.Interval = TimeSpan.FromMilliseconds(DelayInMilliseconds);
            _delayTimer.Tick += HandleDelayElapsed;

            _matches = new ObservableCollection<MatchModel> { new MatchModel(_matchModelMapper, Resources.InitialMatchesMessage) };
        }

        private void HandleDelayElapsed(object sender, EventArgs e)
        {
            Matches = GetMatchModels(_searchText);
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
                //Before rendering matches we would like to update MaxMatchesListHeight,
                //which depends on the current window position.
                OnPropertyChanged("MaxMatchesListHeight");

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

        public double SearchTextBoxHeight
        {
            get
            {
                return _searchTextBoxHeight;
            }
            set
            {
                _searchTextBoxHeight = value;
                OnPropertyChanged("SearchTextBoxHeight");
            }
        }

        public double MaxMatchesListHeight
        {
            get
            {
                //Assume that search text control is positioned at the top of the current window (almost true).
                return _presentationService.GetMaxMatchesListHeight(0, _searchTextBoxHeight);
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
            MatchModel selectedMatch = _presentationService.MoveSelectionUp(Matches, SelectedMatch);
            if (selectedMatch != null)
            {
                SelectedMatch = selectedMatch;
            }
        }

        public void MoveSelectionDown()
        {
            MatchModel selectedMatch = _presentationService.MoveSelectionDown(Matches, SelectedMatch);
            if (selectedMatch != null)
            {
                SelectedMatch = selectedMatch;
            }
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

        private ObservableCollection<MatchModel> GetMatchModels(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return new ObservableCollection<MatchModel>
                           {
                               new MatchModel(_matchModelMapper, Resources.InitialMatchesMessage)
                           };
            }

            List<MatchedFileSystemItem> folderMatches = _navigationAssistant.GetFolderMatches(_rootFolders, searchText);

            List<MatchModel> matchModels = _matchModelMapper.GetMatchModels(folderMatches);
            return new ObservableCollection<MatchModel>(matchModels);
        }

        #endregion
    }
}
