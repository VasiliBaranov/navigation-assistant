using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Core.HookManager;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace WindowsExplorerClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private NotifyIcon _notifyIcon;
        private WindowState _storedWindowState = WindowState.Normal;

        private ViewModel CurrentViewModel
        {
            get { return Resources["ViewModel"] as ViewModel; }
        }

        public MainWindow()
        {
            InitializeComponent();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.BalloonTipText = "The app has been minimized. Click the tray icon to show.";
            _notifyIcon.BalloonTipTitle = "Navigation Assistant";
            _notifyIcon.Text = "Navigation Assistant";
            _notifyIcon.Icon = Properties.Resources.TrayIcon;
            _notifyIcon.Click += NotifyIconClick;
            _notifyIcon.Visible = true;

            HookManager.KeyDown += HandleGlobalKeyDown;
        }

        private void HandleGlobalKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            bool correctCombinationClicked = e.Control && e.Shift && e.KeyCode == Keys.N;
            if (correctCombinationClicked)
            {
                ActivateFromTray();
            }
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
            ActivateFromTray();
        }

        private void HandleDeactivated(object sender, EventArgs e)
        {
            DeactivateToTray();
        }

        private void ActivateFromTray()
        {
            CurrentViewModel.UpdateHostWindow();
            WindowState = _storedWindowState;
            Show();
            Activate();
        }

        private void DeactivateToTray()
        {
            Hide();

            //It's better to set the text to empty here, not in activated,
            //as the Matches list reset (thorugh ViewModel) is invisible then.
            SearchText.Text = string.Empty;
        }

        private void Navigate()
        {
            if (CurrentViewModel.CanNavigate())
            {
                CurrentViewModel.Navigate();
                DeactivateToTray();
            }
        }

        private void HandleActivated(object sender, EventArgs e)
        {
            //Can not use FocusManager.FocusedElement="{Binding ElementName=SearchText}" in XAML,
            //as it will work just for the first loading
            SearchText.Focus();
        }

        //Use PreviewKeyDown, not KeyDown, as TextBox (which is always focused by design) consumes all the arrow keys 
        //(which we would like to handle).
        private void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Navigate();
            }
            //We would like to support navigation in the matches list,
            //but always preserve focus in the search text box, so we manually handle key up/down strokes,
            //instead of moving focus to the matches list.
            else if(e.Key == Key.Up)
            {
                CurrentViewModel.MoveSelectionUp();
            }
            else if (e.Key == Key.Down)
            {
                CurrentViewModel.MoveSelectionDown();
            }
        }

        private void HandleMatchesListMouseUp(object sender, MouseButtonEventArgs e)
        {
            Navigate();
        }
    }
}
