using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using Core.Model;
using Core.Services;
using Core.Services.Implementation;
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

        private bool _matchesChanging;

        #endregion

        #region Data Fields

        private ObservableCollection<MatchModel> _matches;

        private string _searchText;

        private MatchModel _selectedMatch;

        private double _searchTextBoxHeight;

        private double _matchesListBoxWidth;

        private double _matchesListBoxActualWidth;

        private double _maxMatchesListBoxHeight;

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

            Matches = new ObservableCollection<MatchModel> { new MatchModel(_matchModelMapper, Resources.InitialMatchesMessage) };
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

        public ObservableCollection<MatchModel> Matches
        {
            get
            {
                return _matches;
            }
            set
            {
                //We would like to:
                //1. set matches width automatically
                //2. restrict maximum matches height (so that matches list does not fall below the screen)

                //Unfortunately, wpf behaves weirdly:
                //1. it calculates matches width depending just on VISIBLE list items
                //(but some may be hidden with scrolling due to max height restriction)
                //2. wpf recalculates matches width while list scrolling (as new items become visible)
                //This is slow as hell and looks weird.

                //Therefore, we do the following:
                //1. bind viewmodel to matches actual width updates
                //2. set matches width to auto, remove height restriction
                //3. load new matches, let wpf determine width according to ALL matches
                //4. when actual width is updated (see MatchesListBoxActualWidth)
                //  a. set width to actual width (so that wpf will not recalculate it when scrolling)
                //  b. set max height

                _matchesChanging = true;

                //wpf is not able to use this value as width, and it falls back to the default value ("Auto")
                //Wpf does not allow to use string values (and simply pass "Auto")
                MatchesListBoxWidth = -1;
                MaxMatchesListBoxHeight = 10000; //remove height restriction

                _matches = value;
                OnPropertyChanged("Matches");
            }
        }

        public double MaxMatchesListBoxHeight
        {
            get
            {
                return _maxMatchesListBoxHeight;
            }
            set
            {
                _maxMatchesListBoxHeight = value;
                OnPropertyChanged("MaxMatchesListBoxHeight");
            }
        }

        public double MatchesListBoxWidth
        {
            get
            {
                return _matchesListBoxWidth;
            }
            set
            {
                _matchesListBoxWidth = value;
                OnPropertyChanged("MatchesListBoxWidth");
            }
        }

        public double MatchesListBoxActualWidth
        {
            get
            {
                return _matchesListBoxActualWidth;
            }
            set
            {
                _matchesListBoxActualWidth = value;
                OnPropertyChanged("MatchesListBoxActualWidth");
                if (!_matchesChanging)
                {
                    return;
                }

                //This setter is called due to the matches update.
                _matchesChanging = false;

                //Assume that search text control is positioned at the top of the current window (almost true).
                MaxMatchesListBoxHeight = _presentationService.GetMaxMatchesListHeight(0, _searchTextBoxHeight);

                //20 pixels needed to account for a scroll bar
                MatchesListBoxWidth = _matchesListBoxActualWidth + 20;
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

        private void HandleDelayElapsed(object sender, EventArgs e)
        {
            Matches = GetMatchModels(_searchText);
            SelectedMatch = Matches[0];
            _delayTimer.Stop(); //Would like to handle tick just once

            OnPropertyChanged("SearchText");
        }

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
