using System;
using System.Windows.Forms;

namespace NavigationAssistant.Core.Services
{
    public interface IKeyboardListener : IDisposable
    {
        event EventHandler KeyCombinationPressed;

        void StartListening(Keys combinationToListen);

        void StopListening();
    }
}
