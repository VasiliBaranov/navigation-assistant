using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;
using Core.HookManager;
using WindowsExplorerClient.Utilities;
using WindowsExplorerClient.ViewModel;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace WindowsExplorerClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private NotifyIcon _notifyIcon;

        private bool _isCtrlPressed;
        private bool _isShiftPressed;

        private Hotkey hk;

        private NavigationModel CurrentNavigationModel
        {
            get { return Resources["NavigationModel"] as NavigationModel; }
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
            HookManager.KeyPress += HandleGlobalKeyPress;
            HookManager.KeyUp += HandleGlobalKeyUp;

            //RegisterGlobalHotKeys();
        }

        //private void RegisterGlobalHotKeys()
        //{
        //    hk = new Hotkey();
        //    hk.KeyCode = Keys.U;
        //    hk.Control = true;
        //    hk.Shift = true;
        //    hk.Pressed += hk_Pressed;

        //    if (!hk.GetCanRegister(this))
        //    {
        //        throw new InvalidOperationException("Can not register a global hotkey.");
        //    }
        //    else
        //    {
        //        bool success = hk.Register(System.Windows.Application.Current.MainWindow);
        //    }
        //}

        //private void hk_Pressed(object sender, HandledEventArgs e)
        //{
        //    ActivateFromTray();
        //}

        private void HandleGlobalKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LControlKey)
            {
                _isCtrlPressed = false;
            }

            if (e.KeyCode == Keys.LShiftKey)
            {
                _isShiftPressed = false;
            }
        }

        private void HandleGlobalKeyPress(object sender, KeyPressEventArgs e)
        {
            //NOTE: May be implement setting e.Handled in all global handlers?
        }

        private void HandleGlobalKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LControlKey)
            {
                _isCtrlPressed = true;
            }

            if (e.KeyCode == Keys.LShiftKey)
            {
                _isShiftPressed = true;
            }

            bool correctCombinationClicked = _isCtrlPressed && _isShiftPressed && e.KeyCode == Keys.M;
            if (correctCombinationClicked)
            {
                ActivateFromTray();
            }
        }

        private void HandleClose(object sender, CancelEventArgs args)
        {
            _notifyIcon.Dispose();
            _notifyIcon = null;

            //if (hk.Registered)
            //{
            //    hk.Unregister();
            //}
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
            SearchText.Text = string.Empty;
        }

        private void Navigate()
        {
            if (CurrentNavigationModel.CanNavigate())
            {
                CurrentNavigationModel.Navigate();
                DeactivateToTray();
            }
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
            if(e.Key == Key.Up)
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
                return;
            }
        }
    }
}
