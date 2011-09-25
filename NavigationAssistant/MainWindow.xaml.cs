using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;
using Core.HookManager;
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

        private readonly NotifyIcon _notifyIcon;

        private bool _isCtrlPressed;

        private bool _isShiftPressed;

        #endregion

        #region Properties

        private NavigationModel CurrentNavigationModel
        {
            get { return Resources["NavigationModel"] as NavigationModel; }
        }

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            _notifyIcon = new NotifyIcon();
            _notifyIcon.BalloonTipText = Properties.Resources.NotifyIconBalloonText;
            _notifyIcon.BalloonTipTitle = Properties.Resources.NotifyIconBalloonTitle;
            _notifyIcon.Text = Properties.Resources.NotifyIconText;
            _notifyIcon.Icon = Properties.Resources.TrayIcon;
            _notifyIcon.Click += HandleNotifyIconClick;
            _notifyIcon.Visible = true;

            HookManager.KeyDown += HandleGlobalKeyDown;
            HookManager.KeyPress += HandleGlobalKeyPress;
            HookManager.KeyUp += HandleGlobalKeyUp;
        }

        #endregion

        #region Global Key Handlers

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

        #endregion

        #region Private Methods

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

        private void HandleClose(object sender, CancelEventArgs args)
        {
            DeactivateToTray();

            //_notifyIcon.Dispose();
            //_notifyIcon = null;

            args.Cancel = true;
        }

        private void HandleNotifyIconClick(object sender, EventArgs e)
        {
            ActivateFromTray();
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
