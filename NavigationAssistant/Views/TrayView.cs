using System;
using System.Windows.Forms;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Properties;

namespace NavigationAssistant.Views
{
    public class TrayView : ITrayView
    {
        #region Fields

        private Settings _currentSettings;

        private NotifyIcon _notifyIcon;

        private MenuItem _runOnStartupMenuItem;

        public event EventHandler ShowMainClicked;

        public event EventHandler ShowSettingsClicked;

        public event EventHandler ExitClicked;

        #endregion

        public TrayView()
        {
            _notifyIcon = CreateNotifyIcon();
        }

        #region Public Methods

        public void ShowView()
        {
            _notifyIcon.Visible = true;
        }

        public void HideView()
        {
            _notifyIcon.Visible = false;
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        public event EventHandler SettingsChanged;

        public Settings CurrentSettings
        {
            get
            {
                return _currentSettings;
            }
            set
            {
                _currentSettings = value;
                _runOnStartupMenuItem.Checked = _currentSettings.RunOnStartup;
            }
        }

        #endregion

        #region Non Public Methods

        private NotifyIcon CreateNotifyIcon()
        {
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.BalloonTipText = Resources.NotifyIconBalloonText;
            notifyIcon.BalloonTipTitle = Resources.NotifyIconBalloonTitle;
            notifyIcon.Text = Resources.NotifyIconText;
            notifyIcon.Icon = Resources.TrayIcon;
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
            _runOnStartupMenuItem = new MenuItem();

            // Initialize exitMenuItem
            exitMenuItem.Text = Resources.ExitMenuItemText;
            exitMenuItem.Click += HandleExitMenuItemClick;

            // Initialize settingsMenuItem
            settingsMenuItem.Text = Resources.SettingsMenuItemText;
            settingsMenuItem.Click += HandleSettingsMenuItemClick;

            // Initialize startupMenuItem
            _runOnStartupMenuItem.Text = Resources.StartupMenuItemText;
            _runOnStartupMenuItem.Checked = false;
            _runOnStartupMenuItem.Click += HandleRunOnStartupMenuItemClick;

            // Initialize contextMenu
            contextMenu.MenuItems.Add(settingsMenuItem);
            contextMenu.MenuItems.Add(_runOnStartupMenuItem);
            contextMenu.MenuItems.Add(exitMenuItem);

            return contextMenu;
        }

        private void FireEvent(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        #endregion

        #region Event Handlers

        private void HandleExitMenuItemClick(object sender, EventArgs e)
        {
            FireEvent(ExitClicked);
        }

        private void HandleSettingsMenuItemClick(object sender, EventArgs e)
        {
            FireEvent(ShowSettingsClicked);
        }

        private void HandleRunOnStartupMenuItemClick(object sender, EventArgs e)
        {
            MenuItem startupMenuItem = sender as MenuItem;
            startupMenuItem.Checked = !startupMenuItem.Checked;

            _currentSettings.RunOnStartup = startupMenuItem.Checked;
            FireEvent(SettingsChanged);
        }

        //This method is subscribed to the MouseClick event. We can not subscribe to the Click event,
        //as then the handler will be closed even if context menu items are clicked, and before them.
        //This handler is also called before menu item handlers, but e.Button is Right for menu clicks.
        private void HandleNotifyIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FireEvent(ShowMainClicked);
            }
        }

        #endregion
    }
}
