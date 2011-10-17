using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Utilities;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.Properties;
using NavigationAssistant.ViewModel;
using NavigationAssistant.Views;

namespace NavigationAssistant.Presenters.Implementation
{
    /// <summary>
    /// Implements a presenter for the Navigation View (i.e. the main window of the application).
    /// </summary>
    public class NavigationPresenter : BasePresenter, IPresenter
    {
        #region Events

        public event EventHandler<ItemEventArgs<Settings>> SettingsChanged;

        public event EventHandler<ItemEventArgs<Type>> RequestWindowShow;

        public event EventHandler Exited;

        #endregion

        #region Services

        private readonly ISettingsSerializer _settingsSerializer;

        private readonly IKeyboardListener _keyboardListener;

        private INavigationService _navigationAssistant;

        private readonly INavigationServiceBuilder _navigationServiceBuilder;

        private readonly IMatchModelMapper _matchModelMapper;

        #endregion

        #region Fields

        private readonly INavigationView _view;

        private ApplicationWindow _hostWindow;

        private readonly object _syncObject = new object();

        private bool _disposed;

        private delegate INavigationService InitializeDelegate(Settings settings);

        #endregion

        #region Constructors

        public NavigationPresenter(INavigationView view,
            ISettingsSerializer settingsSerializer,
            IKeyboardListener keyboardListener,
            IMatchModelMapper matchModelMapper,
            INavigationServiceBuilder navigationServiceBuilder)
        {
            _view = view;
            _settingsSerializer = settingsSerializer;
            _keyboardListener = keyboardListener;
            _matchModelMapper = matchModelMapper;
            _navigationServiceBuilder = navigationServiceBuilder;

            Settings settings = _settingsSerializer.Deserialize();

            _keyboardListener.KeyCombinationPressed += GlobalKeyCombinationPressed;
            _keyboardListener.StartListening(settings.GlobalKeyCombination);

            _view.CurrentSettings = settings;
            _view.ShowMatches(new List<MatchModel> {new MatchModel(_matchModelMapper, Resources.InitialMatchesMessage)});
            _view.ShowInitializingScreen = true;

            //Initialize navigation service asynchronously, as it may require a long operation (file system parsing).
            //Clone settings to avoid any coupling
            Settings settingsCopy = settings.Clone() as Settings;
            InitializeDelegate initialize = Initialize;
            initialize.BeginInvoke(settingsCopy, EndInitialize, initialize);
        }

        #endregion

        #region Public Methods

        public void UpdateSettings(Settings settings)
        {
            lock (_syncObject)
            {
                _view.CurrentSettings = settings;

                _keyboardListener.StopListening();
                _keyboardListener.StartListening(settings.GlobalKeyCombination);

                if (_navigationAssistant != null)
                {
                    //This code will not be called, if _navigationServiceBuilder is used in the
                    //Initialize function; so we don't need to synchronize _navigationServiceBuilder additionally.
                    _navigationServiceBuilder.UpdateNavigationSettings(_navigationAssistant, settings);
                }
            }
        }

        public void Show()
        {
            lock (_syncObject)
            {
                _view.ShowView();
            }
        }

        public void Dispose()
        {
            lock (_syncObject)
            {
                _disposed = true;

                _view.Dispose();
                _keyboardListener.Dispose();

                if (_navigationAssistant != null)
                {
                    _navigationAssistant.Dispose();
                }
            }
        }

        #endregion

        #region Non Public Methods

        private INavigationService Initialize(Settings settings)
        {
            //Should not lock this operation, as otherwise all other methods will be locked.
            //_navigationServiceBuilder can not be used anywhere else.
            return _navigationServiceBuilder.BuildNavigationService(settings);
        }

        private void EndInitialize(IAsyncResult asyncResult)
        {
            InitializeDelegate initializeDelegate = (InitializeDelegate)asyncResult.AsyncState;
            INavigationService navigationAssistant = initializeDelegate.EndInvoke(asyncResult);

            lock (_syncObject)
            {
                if (_disposed)
                {
                    navigationAssistant.Dispose();
                    return;
                }

                _navigationAssistant = navigationAssistant;
                _navigationServiceBuilder.UpdateNavigationSettings(_navigationAssistant, _view.CurrentSettings);

                _view.ShowInitializingScreen = false;
                _view.TextChanged += HandleTextChanged;
                _view.FolderSelected += HandleFolderSelected;
            }
        }

        private void GlobalKeyCombinationPressed(object sender, EventArgs e)
        {
            lock (_syncObject)
            {
                if (_navigationAssistant != null)
                {
                    _hostWindow = _navigationAssistant.GetActiveWindow();
                }

                _view.ShowView();
            }
        }

        //Called just if initialized
        private void HandleFolderSelected(object sender, ItemEventArgs<string> e)
        {
            string folderPath = e.Item;

            bool canNavigate = !string.IsNullOrEmpty(folderPath) && _hostWindow != null;

            if (canNavigate)
            {
                _navigationAssistant.NavigateTo(folderPath, _hostWindow);
                _view.HideView();
            }
        }

        //Called just if initialized
        private void HandleTextChanged(object sender, ItemEventArgs<string> e)
        {
            List<MatchModel> matches = GetMatchModels(e.Item);

            _view.ShowMatches(matches);
        }

        //Called just if initialized
        private List<MatchModel> GetMatchModels(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return new List<MatchModel>
                           {
                               new MatchModel(_matchModelMapper, Resources.InitialMatchesMessage)
                           };
            }

            List<MatchedFileSystemItem> folderMatches = _navigationAssistant.GetFolderMatches(searchText);

            List<MatchModel> matchModels = _matchModelMapper.GetMatchModels(folderMatches);
            return matchModels;
        }

        #endregion
    }
}
