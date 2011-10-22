using System;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Views
{
    /// <summary>
    /// Defines methods for all the views in the application.
    /// </summary>
    public interface IView : IDisposable
    {
        //Names chosen to avoid conflicts with standard Window methods
        void ShowView();

        void HideView();

        event EventHandler SettingsChanged;

        Settings CurrentSettings { get; set; }
    }
}
