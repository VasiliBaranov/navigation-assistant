using System;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Presenters
{
    /// <summary>
    /// Defines methods and events for a presenter (from Model-View-Presenter pattern).
    /// </summary>
    public interface IPresenter : IDisposable
    {
        event EventHandler<ItemEventArgs<Settings>> SettingsChanged;

        event EventHandler<ItemEventArgs<Type>> RequestWindowShow;

        event EventHandler Exited;

        void UpdateSettings(Settings settings);

        void Show();
    }
}
