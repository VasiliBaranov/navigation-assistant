using System;

namespace NavigationAssistant.Presenters
{
    public interface IPresenter : IDisposable
    {
        event EventHandler SettingsChanged;

        event EventHandler<RequestShowingEventArgs> RequestShowing;

        event EventHandler Exited;

        void UpdateSettings();

        void Show();
    }
}
