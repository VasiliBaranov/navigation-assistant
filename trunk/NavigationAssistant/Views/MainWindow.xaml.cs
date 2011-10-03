using System;
using System.ComponentModel;
using System.Windows.Input;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.PresentationServices.Implementations;
using NavigationAssistant.Presenters;
using NavigationAssistant.ViewModel;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace NavigationAssistant.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields

        private readonly ISettingsSerializer _settingsSerializer;

        private readonly IKeyboardListener _keyboardListener;

        private readonly IPresentationService _presentationService;

        private readonly IPresenter _trayIconPresenter;

        private bool _closingCompletely;

        #endregion

        #region Properties

        public NavigationModel CurrentNavigationModel
        {
            get { return Resources["NavigationModel"] as NavigationModel; }
        }

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            _settingsSerializer = new SettingsSerializer();

            _keyboardListener = new KeyboardListener();
            _keyboardListener.KeyCombinationPressed += GlobalKeyCombinationPressed;
            _keyboardListener.StartListening(_settingsSerializer.Deserialize().GlobalKeyCombination);

            _presentationService = new PresentationService();

            _trayIconPresenter = new TrayIconPresenter(new TrayView(), new SettingsSerializer());
            _trayIconPresenter.Exited += HandleExited;
            _trayIconPresenter.RequestShowing += HandleRequestShowing;
            _trayIconPresenter.Show();

            DeactivateToTray();
        }

        #endregion

        public void UpdateKeyListening()
        {
            _keyboardListener.StopListening();
            _keyboardListener.StartListening(_settingsSerializer.Deserialize().GlobalKeyCombination);
        }

        public void UpdateIconMenu()
        {
            _trayIconPresenter.UpdateSettings();
        }

        #region Private Methods

        private void ActivateFromTray()
        {
            CurrentNavigationModel.UpdateHostWindow();

            _presentationService.MakeForeground(this);
        }

        private void DeactivateToTray()
        {
            Hide();

            //It's better to set the text to empty here, not in activated,
            //as the Matches list reset (thorugh NavigationModel) is invisible then.
            SearchTextBox.Text = string.Empty;
        }

        private void Navigate()
        {
            if (CurrentNavigationModel.CanNavigate())
            {
                CurrentNavigationModel.Navigate();
                DeactivateToTray();
            }
        }

        #endregion

        #region Event Handlers

        private void HandleRequestShowing(object sender, RequestShowingEventArgs e)
        {
            if (e.PresenterToShow == typeof(SettingsPresenter))
            {
                //Don't close the main window, as it should be hidden (due to deactivation) when the tray menu appears.
                SettingsWindow settings = new SettingsWindow();
                settings.Show();
                return;
            }

            if (e.PresenterToShow == typeof(NavigationPresenter))
            {
                ActivateFromTray();
                return;
            }
        }

        private void HandleExited(object sender, EventArgs e)
        {
            _keyboardListener.StopListening();
            CurrentNavigationModel.Close();

            _trayIconPresenter.Dispose();

            _closingCompletely = true;
            Close();
        }

        private void GlobalKeyCombinationPressed(object sender, EventArgs e)
        {
            ActivateFromTray();
        }

        private void HandleClose(object sender, CancelEventArgs args)
        {
            if (_closingCompletely)
            {
                return;
            }

            DeactivateToTray();
            args.Cancel = true;
        }

        private void HandleDeactivated(object sender, EventArgs e)
        {
            DeactivateToTray();
        }

        //We can not subscribe to ListItem (Label) events, as list items are aligned to the left
        //(to determine the width of the listbox correctly) and shorter labels do not occupy the entire line.
        //Also, we can not subscribe to MouseDown, as SelectedItem is changed just between MouseDown and MouseUp.
        private void HandleMatchesListMouseUp(object sender, MouseButtonEventArgs e)
        {
            Navigate();
        }

        //Use PreviewKeyDown, not KeyDown, as TextBox (which is always focused by design) consumes all the arrow keys 
        //(which we would like to handle).
        private void HandleSearchTextPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //We would like to support navigation in the matches list,
            //when focus is in the search text box, so we manually handle key up/down strokes,
            //instead of moving focus to the matches list.

            //Also, you can not use this handler over the entire panel, because if focus were in the ListBox,
            //the following handlers sequence would occur:
            //manual selection change in the preview key down handler, 
            //attempt to change selection by the ListBox in the key down handler.
            //It causes incorrect ListBox behaviour, as selection in its handler is different from the initial state.
            if (e.Key == Key.Up)
            {
                CurrentNavigationModel.MoveSelectionUp();
            }
            else if (e.Key == Key.Down)
            {
                CurrentNavigationModel.MoveSelectionDown();
            }
        }

        private void HandlePanelKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Navigate();
            }
        }

        private void HandleWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DeactivateToTray();
            }
        }

        #endregion

    }
}
