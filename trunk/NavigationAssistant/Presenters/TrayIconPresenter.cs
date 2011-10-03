using System;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Views;

namespace NavigationAssistant.Presenters
{
    public class TrayIconPresenter : IPresenter
    {
        #region Fields

        public event EventHandler SettingsChanged;

        public event EventHandler<RequestShowingEventArgs> RequestShowing;

        public event EventHandler Exited;

        private readonly ITrayView _view;

        private readonly ISettingsSerializer _settingsSerializer;

        #endregion

        #region Constructors

        public TrayIconPresenter(ITrayView view, ISettingsSerializer settingsSerializer)
        {
            _settingsSerializer = settingsSerializer;
            _view = view;
            _view.CurrentSettings = _settingsSerializer.Deserialize();

            _view.ExitClicked += HandleExitClicked;
            _view.SettingsChanged += HandleSettingsChanged;
            _view.ShowMainClicked += HandleShowMainClicked;
            _view.ShowSettingsClicked +=HandleShowSettingsClicked;
        }

        #endregion

        #region Public Methods

        public void UpdateSettings()
        {
            _view.CurrentSettings = _settingsSerializer.Deserialize();
        }

        public void Show()
        {
            _view.Show();
        }

        public void Dispose()
        {
            _view.ExitClicked -= HandleExitClicked;
            _view.SettingsChanged -= HandleSettingsChanged;
            _view.ShowMainClicked -= HandleShowMainClicked;
            _view.ShowSettingsClicked -= HandleShowSettingsClicked;

            _view.Dispose();

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Non Public Methods

        private void HandleShowMainClicked(object sender, EventArgs e)
        {
            RequestShowingEventArgs eventArgs = new RequestShowingEventArgs(typeof(NavigationPresenter));
            FireEvent(RequestShowing, eventArgs);
        }

        private void HandleSettingsChanged(object sender, EventArgs e)
        {
            _settingsSerializer.Serialize(_view.CurrentSettings);
            FireEvent(SettingsChanged);
        }

        private void HandleExitClicked(object sender, EventArgs e)
        {
            FireEvent(Exited);
        }

        private void HandleShowSettingsClicked(object sender, EventArgs e)
        {
            RequestShowingEventArgs eventArgs = new RequestShowingEventArgs(typeof(SettingsPresenter));
            FireEvent(RequestShowing, eventArgs);
        }

        private void FireEvent(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void FireEvent<T>(EventHandler<T> handler, T args) where T : EventArgs
        {
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #endregion
    }
}
