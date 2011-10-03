using System;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Utilities;

namespace NavigationAssistant.Presenters
{
    public interface IPresenter : IDisposable
    {
        event EventHandler<ItemEventArgs<Settings>> SettingsChanged;

        event EventHandler<ItemEventArgs<Type>> RequestWindowShow;

        event EventHandler Exited;

        void UpdateSettings(Settings settings);

        void Show();
    }
}
