using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.PresentationServices.Implementations;
using NavigationAssistant.ViewModel;

namespace NavigationAssistant.Views.Implementation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NavigationWindow : INavigationView
    {
        #region Event Handlers

        public event EventHandler SettingsChanged;

        public event EventHandler<ItemEventArgs<string>> TextChanged;

        public event EventHandler<ItemEventArgs<string>> FolderSelected;

        #endregion

        #region Fields

        private bool _closingCompletely;

        private readonly IPresentationService _presentationService;

        private readonly NavigationModel _viewModel;

        private readonly DispatcherTimer _delayTimer;

        private const int DelayInMilliseconds = 200;

        #endregion

        #region Properties

        public Settings CurrentSettings { get; set; }

        public bool ShowInitializingScreen
        {
            get { return _viewModel.ShowInitializingScreen; }
            set { _viewModel.ShowInitializingScreen = value; }
        }

        #endregion

        #region Constructors

        public NavigationWindow()
        {
            InitializeComponent();

            _presentationService = new PresentationService();

            _viewModel = new NavigationModel(_presentationService);
            _viewModel.SearchTextChanged += HandleSearchTextChanged;
            DataContext = _viewModel;

            _delayTimer = new DispatcherTimer();
            _delayTimer.Interval = TimeSpan.FromMilliseconds(DelayInMilliseconds);
            _delayTimer.Tick += HandleDelayElapsed;

            HideView();
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            _closingCompletely = true;
            Close();
        }

        public void ShowMatches(List<MatchModel> matches)
        {
            _viewModel.Matches = new ObservableCollection<MatchModel>(matches);
            _viewModel.SelectedMatch = matches[0];
        }

        public void ShowView()
        {
            _presentationService.MakeForeground(this);
        }

        public void HideView()
        {
            Hide();

            //It's better to set the text to empty here, not in activated,
            //as the Matches list reset (thorugh NavigationModel) is invisible then.
            SearchTextBox.Text = string.Empty;
        }

        #endregion

        #region Private Methods

        private void MoveSelectionUp()
        {
            MatchModel selectedMatch = _presentationService.MoveSelectionUp(_viewModel.Matches, _viewModel.SelectedMatch);
            UpdateSelectedMatch(selectedMatch);
        }

        private void MoveSelectionDown()
        {
            MatchModel selectedMatch = _presentationService.MoveSelectionDown(_viewModel.Matches, _viewModel.SelectedMatch);
            UpdateSelectedMatch(selectedMatch);
        }

        private void UpdateSelectedMatch(MatchModel selectedMatch)
        {
            if (selectedMatch == null)
            {
                return;
            }

            _viewModel.SelectedMatch = selectedMatch;
        }

        private void Navigate()
        {
            FireEvent(FolderSelected, new ItemEventArgs<string>(_viewModel.SelectedMatch.Path));
        }

        private void FireEvent<T>(EventHandler<T> handler, T args) where T : EventArgs
        {
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #endregion

        #region Event Handlers

        private void HandleSearchTextChanged(object sender, EventArgs e)
        {
            //To avoid too frequent updates we run a timer to take a pause.
            //If search text is not updated during this pause, matches will be rendered.

            //Need to stop the execution from the previous setter.
            _delayTimer.Stop();
            _delayTimer.Start();
        }

        private void HandleDelayElapsed(object sender, EventArgs e)
        {
            _delayTimer.Stop(); //Would like to handle tick just once

            FireEvent(TextChanged, new ItemEventArgs<string>(_viewModel.SearchText));
        }

        private void HandleClose(object sender, CancelEventArgs args)
        {
            if (_closingCompletely)
            {
                return;
            }

            HideView();
            args.Cancel = true;
        }

        private void HandleDeactivated(object sender, EventArgs e)
        {
            HideView();
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
                MoveSelectionUp();
            }
            else if (e.Key == Key.Down)
            {
                MoveSelectionDown();
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
                HideView();
            }
        }

        #endregion
    }
}
