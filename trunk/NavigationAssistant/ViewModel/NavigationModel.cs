using System;
using System.Collections.ObjectModel;
using System.Windows;
using NavigationAssistant.PresentationServices;

namespace NavigationAssistant.ViewModel
{
    /// <summary>
    /// Implements a view model for the main navigation window.
    /// </summary>
    public class NavigationModel : BaseViewModel
    {
        #region Supplemetary Fields

        private readonly IPresentationService _presentationService;

        public event EventHandler SearchTextChanged;

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

        private bool _showInitializingScreen;

        #endregion

        #region Constructors

        public NavigationModel(IPresentationService presentationService)
        {
            _presentationService = presentationService;
        }

        #endregion

        #region Properties

        public bool ShowInitializingScreen
        {
            get
            {
                return _showInitializingScreen;
            }
            set
            {
                _showInitializingScreen = value;
                OnPropertyChanged("ShowInitializingScreen");
                OnPropertyChanged("InitializingScreenVisiblity");
                OnPropertyChanged("MainScreenVisiblity");
                OnPropertyChanged("SearchTextFocused");
            }
        }

        public Visibility InitializingScreenVisiblity
        {
            get
            {
                return _showInitializingScreen ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility MainScreenVisiblity
        {
            get
            {
                return _showInitializingScreen ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool SearchTextFocused
        {
            get { return !_showInitializingScreen; }
        }

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
                FireEvent(SearchTextChanged);
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
                    _selectedMatch.IsSelected = false;
                }

                _selectedMatch = value;

                if (_selectedMatch != null)
                {
                    _selectedMatch.IsSelected = true;
                }

                OnPropertyChanged("SelectedMatch");
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

                //Assume that search text control is positioned at the topleft corner of the current window (almost true).
                MaxMatchesListBoxHeight = _presentationService.GetMaxMatchesListHeight(0, _searchTextBoxHeight);

                //Set up matches listbox width explicitly, also taking into account max available screen width.
                //20 pixels needed to account for a scroll bar.
                double maxMatchesListWidth = _presentationService.GetMaxMatchesListWidth(0);
                MatchesListBoxWidth = Math.Min(_matchesListBoxActualWidth + 20, maxMatchesListWidth);
            }
        }

        #endregion
    }
}
