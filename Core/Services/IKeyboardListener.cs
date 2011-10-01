using System;
using System.Windows.Forms;

namespace Core.Services
{
    public interface IKeyboardListener
    {
        event EventHandler KeyCombinationPressed;

        void StartListening(Keys combinationToListen);

        void StopListening();
    }
}
