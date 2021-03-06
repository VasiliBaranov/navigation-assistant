﻿using System;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Utilities;
using NavigationAssistant.Views;

namespace NavigationAssistant.Presenters.Implementation
{
    /// <summary>
    /// Implements a presenter for the Settings View.
    /// </summary>
    public class SettingsPresenter : BasePresenter, IPresenter
    {
        #region Fields

        public event EventHandler<ItemEventArgs<Settings>> SettingsChanged;

        public event EventHandler<ItemEventArgs<Type>> RequestWindowShow;

        public event EventHandler Exited;

        private readonly ISettingsView _view;

        private readonly ISettingsSerializer _settingsSerializer;

        #endregion

        #region Constructors

        public SettingsPresenter(ISettingsView view, ISettingsSerializer settingsSerializer)
        {
            _settingsSerializer = settingsSerializer;

            _view = view;
            _view.CurrentSettings = _settingsSerializer.Deserialize();
            _view.SettingsChanged += HandleSettingsChanged;
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
            _view.Dispose();
        }

        #endregion

        #region Non Public Methods

        private void HandleSettingsChanged(object sender, EventArgs e)
        {
            ValidationResult validationResult = _settingsSerializer.Serialize(_view.CurrentSettings);

            if (validationResult.IsValid)
            {
                _view.HideView();
                FireEvent(SettingsChanged, _view.CurrentSettings);
            }
            else
            {
                _view.ShowValidationResult(validationResult);
            }
        }

        #endregion
    }
}
