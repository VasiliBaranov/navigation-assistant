using System;

namespace NavigationAssistant.Views
{
    public interface ITrayView : IView
    {
        event EventHandler ShowMainClicked;

        event EventHandler ShowSettingsClicked;

        event EventHandler ExitClicked;
    }
}
