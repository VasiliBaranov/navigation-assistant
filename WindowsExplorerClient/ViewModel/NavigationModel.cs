using System;
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
        #region Fields

        private readonly INavigationAssistant _navigationAssistant;

        private readonly IMatchModelMapper _matchModelMapper;

        private readonly DispatcherTimer _delayTimer;

        private ObservableCollection<MatchModel> _matches;

        private string _searchText;

        private MatchModel _selectedMatch;

        public event PropertyChangedEventHandler PropertyChanged;

        private const int DelayInMilliseconds = 200;

        #endregion

        #region Constructors

        public NavigationModel()
        {
            IFileSystemParser fileSystemParser = new CachedFileSystemParser(new FileSystemParser(), new CacheSerializer(@"e:\temp\Cache.txt"));
            //_navigationAssistant = new NavigationAssistant(fileSystemParser, new MatchSearcher(), new WindowsExplorerManager());
            _navigationAssistant = new NavigationAssistant(fileSystemParser, new MatchSearcher(), new TotalCommanderManager(@"d:\Program Files\Total Commander\TOTALCMD.EXE"));

            _matchModelMapper = new MatchModelMapper(_navigationAssistant);

            _delayTimer = new DispatcherTimer();
            _delayTimer.Interval = TimeSpan.FromMilliseconds(DelayInMilliseconds);
            _delayTimer.Tick += HandleDelayElapsed;

            _matches = new ObservableCollection<MatchModel> { new MatchModel(_matchModelMapper, Resources.InitialMatchesMessage) };
        }

        private void HandleDelayElapsed(object sender, EventArgs e)
        {
            Matches = new ObservableCollection<MatchModel>(_matchModelMapper.GetMatchModels(_searchText));
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

        #endregion
    }
}
