using System;
using System.ComponentModel;
using System.Windows;

namespace WindowsExplorerClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private WindowState _storedWindowState = WindowState.Normal;

        public MainWindow()
        {
            InitializeComponent();

            // initialise code here
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.BalloonTipText = "The app has been minimized. Click the tray icon to show.";
            _notifyIcon.BalloonTipTitle = "Navigation Assistant";
            _notifyIcon.Text = "Navigation Assistant";
            _notifyIcon.Icon = Properties.Resources.TrayIcon;
            _notifyIcon.Click += NotifyIconClick;
            _notifyIcon.Visible = true;
        }

        private void HandleClose(object sender, CancelEventArgs args)
        {
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        private void HandleStateChanged(object sender, EventArgs args)
        {
            //Actually, currently this handler is redundand, as we support just normal state,
            //but it's left for generality.
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            else
            {
                _storedWindowState = WindowState;
            }
        }

        private void NotifyIconClick(object sender, EventArgs e)
        {
            WindowState = _storedWindowState;
            Show();
            Activate();
        }

        private void HandleDeactivated(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
