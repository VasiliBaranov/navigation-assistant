using System;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Utilities;
using NavigationAssistant.Views;

namespace NavigationAssistant.Presenters.Implementation
{
    public class TrayIconPresenter : BasePresenter, IPresenter
    {
        #region Fields

        public event EventHandler<ItemEventArgs<Settings>> SettingsChanged;

        public event EventHandler<ItemEventArgs<Type>> RequestWindowShow;

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

            //Show tray icon at once
            _view.ShowView();
        }

        #endregion

        #region Public Methods

        public void UpdateSettings(Settings settings)
        {
            _view.CurrentSettings = settings;
        }

        public void Show()
        {
            _view.ShowView();
        }

        public void Dispose()
        {
            _view.ExitClicked -= HandleExitClicked;
            _view.SettingsChanged -= HandleSettingsChanged;
            _view.ShowMainClicked -= HandleShowMainClicked;
            _view.ShowSettingsClicked -= HandleShowSettingsClicked;

            _view.Dispose();
        }

        #endregion

        #region Non Public Methods

        private void HandleShowMainClicked(object sender, EventArgs e)
        {
            FireEvent(RequestWindowShow, typeof(NavigationPresenter));
        }

        private void HandleSettingsChanged(object sender, EventArgs e)
        {
            _settingsSerializer.Serialize(_view.CurrentSettings);
            FireEvent(SettingsChanged, _view.CurrentSettings);
        }

        private void HandleExitClicked(object sender, EventArgs e)
        {
            FireEvent(Exited);
        }

        private void HandleShowSettingsClicked(object sender, EventArgs e)
        {
            FireEvent(RequestWindowShow, typeof(SettingsPresenter));
        }

        #endregion
    }
}
