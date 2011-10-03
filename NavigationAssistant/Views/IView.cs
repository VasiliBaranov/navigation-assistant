using System;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Views
{
    public interface IView : IDisposable
    {
        //Names chosen to avoid conflicts with standard Window methods
        void ShowView();

        void HideView();

        event EventHandler SettingsChanged;

        Settings CurrentSettings { get; set; }
    }
}
