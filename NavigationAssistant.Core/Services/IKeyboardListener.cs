using System;
using System.Windows.Forms;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines methods and events for listening to the pressing of a specified key combination globally (i.e. among all the windows).
    /// </summary>
    public interface IKeyboardListener : IDisposable
    {
        event EventHandler KeyCombinationPressed;

        void StartListening(Keys combinationToListen);

        void StopListening();
    }
}
