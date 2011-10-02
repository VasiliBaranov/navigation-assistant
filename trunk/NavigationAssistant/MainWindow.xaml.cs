using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.ViewModel;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace NavigationAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields

        private NotifyIcon _notifyIcon;

        private bool _isClosingCompletely;

        private readonly ISettingsSerializer _settingsSerializer;

        private readonly IKeyboardListener _keyboardListener;

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

            _notifyIcon = CreateNotifyIcon();
            _notifyIcon.Visible = true;

            DeactivateToTray();
        }

        #endregion

        public void UpdateKeyListening()
        {
            _keyboardListener.StopListening();
            _keyboardListener.StartListening(_settingsSerializer.Deserialize().GlobalKeyCombination);
        }

        #region Private Methods

        private NotifyIcon CreateNotifyIcon()
        {
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.BalloonTipText = Properties.Resources.NotifyIconBalloonText;
            notifyIcon.BalloonTipTitle = Properties.Resources.NotifyIconBalloonTitle;
            notifyIcon.Text = Properties.Resources.NotifyIconText;
            notifyIcon.Icon = Properties.Resources.TrayIcon;
            notifyIcon.MouseClick += HandleNotifyIconClick;

            // The ContextMenu property sets the menu that will
            // appear when the systray icon is right clicked.
            notifyIcon.ContextMenu = CreateIconContextMenu();

            return notifyIcon;
        }

        private ContextMenu CreateIconContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem exitMenuItem = new MenuItem();
            MenuItem settingsMenuItem = new MenuItem();
            MenuItem runOnStartupMenuItem = new MenuItem();

            // Initialize exitMenuItem
            exitMenuItem.Text = Properties.Resources.ExitMenuItemText;
            exitMenuItem.Click += HandleExitMenuItemClick;

            // Initialize settingsMenuItem
            settingsMenuItem.Text = Properties.Resources.SettingsMenuItemText;
            settingsMenuItem.Click += HandleSettingsMenuItemClick;

            // Initialize startupMenuItem
            runOnStartupMenuItem.Text = Properties.Resources.StartupMenuItemText;
            runOnStartupMenuItem.Checked = _settingsSerializer.GetRunOnStartup();
            runOnStartupMenuItem.Click += HandleRunOnStartupMenuItemClick;

            // Initialize contextMenu
            contextMenu.MenuItems.Add(settingsMenuItem);
            contextMenu.MenuItems.Add(runOnStartupMenuItem);
            contextMenu.MenuItems.Add(exitMenuItem);

            return contextMenu;
        }

        //This method will be moved to icon view and presenter
        public void UpdateIconMenu()
        {
            _notifyIcon.ContextMenu.MenuItems[1].Checked = _settingsSerializer.GetRunOnStartup();
        }

        private void ActivateFromTray()
        {
            CurrentNavigationModel.UpdateHostWindow();

            //Both calls are necessary, as visibility and being a foreground window are independent
            Show();
            Activate();
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

        #region Menu Handlers

        private void HandleExitMenuItemClick(object sender, EventArgs e)
        {
            _isClosingCompletely = true;

            Close();
        }

        private void HandleSettingsMenuItemClick(object sender, EventArgs e)
        {
            //Don't close the main window, as it should be hidden (due to deactivation) when the tray menu appears.

            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        private void HandleRunOnStartupMenuItemClick(object sender, EventArgs e)
        {
            MenuItem startupMenuItem = sender as MenuItem;
            startupMenuItem.Checked = !startupMenuItem.Checked;

            _settingsSerializer.SetRunOnStartup(startupMenuItem.Checked);
        }

        #endregion

        private void GlobalKeyCombinationPressed(object sender, EventArgs e)
        {
            ActivateFromTray();
        }

        private void HandleClose(object sender, CancelEventArgs args)
        {
            if (_isClosingCompletely)
            {
                _notifyIcon.Dispose();
                _notifyIcon = null;

                _keyboardListener.StopListening();
            }
            else
            {
                DeactivateToTray();
                args.Cancel = true;
            }
        }

        //This method is subscribed to the MouseClick event. We can not subscribe to the Click event,
        //as then the handler will be closed even if context menu items are clicked, and before them.
        //This handler is also called before menu item handlers, but e.Button is Right for menu clicks.
        private void HandleNotifyIconClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ActivateFromTray();
            }
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
