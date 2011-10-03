using System;
using System.Collections.Generic;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.Properties;
using NavigationAssistant.Utilities;
using NavigationAssistant.ViewModel;
using NavigationAssistant.Views;

namespace NavigationAssistant.Presenters
{
    public class NavigationPresenter : BasePresenter, IPresenter
    {
        #region Fields

        public event EventHandler<ItemEventArgs<Settings>> SettingsChanged;
        public event EventHandler<ItemEventArgs<Type>> RequestWindowShow;
        public event EventHandler Exited;

        private readonly INavigationView _view;

        private readonly ISettingsSerializer _settingsSerializer;

        private readonly IKeyboardListener _keyboardListener;

        private readonly IPresentationService _presentationService;

        private readonly IPresenter _trayIconPresenter;

        private INavigationService _navigationAssistant;

        private readonly IMatchModelMapper _matchModelMapper;

        #endregion

        #region Fields

        private ApplicationWindow _hostWindow;

        #endregion

        #region Constructors

        public NavigationPresenter(INavigationView view, 
            ISettingsSerializer settingsSerializer, 
            IKeyboardListener keyboardListener, 
            IPresentationService presentationService, 
            IPresenter trayIconPresenter,
            IMatchModelMapper matchModelMapper)
        {
            _view = view;
            _settingsSerializer = settingsSerializer;
            _keyboardListener = keyboardListener;
            _presentationService = presentationService;
            _trayIconPresenter = trayIconPresenter;
            _matchModelMapper = matchModelMapper;

            _trayIconPresenter.Exited += HandleExited;
            _trayIconPresenter.RequestWindowShow += HandleRequestWindowShow;
            _trayIconPresenter.Show();

            _view.CurrentSettings = _settingsSerializer.Deserialize();
            _view.TextChanged += HandleTextChanged;
            _view.FolderSelected += HandleFolderSelected;

            _keyboardListener.KeyCombinationPressed += GlobalKeyCombinationPressed;
            _keyboardListener.StartListening(_view.CurrentSettings.GlobalKeyCombination);

            _navigationAssistant = _settingsSerializer.BuildNavigationService(_view.CurrentSettings);

            _view.ShowMatches(new List<MatchModel> { new MatchModel(_matchModelMapper, Resources.InitialMatchesMessage) });
        }

        private void HandleExited(object sender, EventArgs e)
        {
            Dispose();
        }

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

        private void HandleTextChanged(object sender, ItemEventArgs<string> e)
        {
            List<MatchModel> matches = GetMatchModels(e.Item);

            _view.ShowMatches(matches);
        }

        #endregion

        public void UpdateSettings(Settings settings)
        {
            _view.CurrentSettings = settings;

            _keyboardListener.StopListening();
            _keyboardListener.StartListening(settings.GlobalKeyCombination);

            _trayIconPresenter.UpdateSettings(settings);

            if (_navigationAssistant != null)
            {
                _navigationAssistant.Dispose();
            }

            _navigationAssistant = _settingsSerializer.BuildNavigationService(settings);
        }

        public void Show()
        {
            _view.ShowView();
        }

        public void Dispose()
        {
            _view.Dispose();
            _trayIconPresenter.Dispose();
            _navigationAssistant.Dispose();

            _keyboardListener.StopListening();

            GC.SuppressFinalize(this);
        }

        private void GlobalKeyCombinationPressed(object sender, EventArgs e)
        {
            _hostWindow = _navigationAssistant.GetActiveWindow();

            _view.ShowView();
        }

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

        private void HandleRequestWindowShow(object sender, ItemEventArgs<Type> e)
        {
            if (e.Item == typeof(SettingsPresenter))
            {
                //Don't close the main window, as it should be hidden (due to deactivation) when the tray menu appears.
                SettingsWindow settings = new SettingsWindow();
                settings.Show();
                return;
            }

            if (e.Item == typeof(NavigationPresenter))
            {
                _view.ShowView();
                return;
            }
        }
    }
}
