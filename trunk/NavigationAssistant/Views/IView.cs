using System;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Views
{
    public interface IView : IDisposable
    {
        void Show();

        event EventHandler SettingsChanged;

        Settings CurrentSettings { get; set; }
    }
}
