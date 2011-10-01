using System;
using Core.Model;

namespace Core.Services
{
    public interface IKeyboardListener
    {
        event EventHandler KeyCombinationPressed;

        void StartListening(KeyCombination combinationToListen);

        void StopListening();
    }
}
