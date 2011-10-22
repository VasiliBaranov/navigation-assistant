using System;

namespace NavigationAssistant.Views
{
    /// <summary>
    /// Defines methods for the tray icon view.
    /// </summary>
    public interface ITrayView : IView
    {
        event EventHandler ShowMainClicked;

        event EventHandler ShowSettingsClicked;

        event EventHandler ExitClicked;
    }
}
