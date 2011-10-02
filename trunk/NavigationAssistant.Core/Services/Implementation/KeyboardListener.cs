using System;
using System.Windows.Forms;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class KeyboardListener : IKeyboardListener
    {
        #region Fields

        private Keys _combinationToListen;

        private Keys _activeCombination;

        public event EventHandler KeyCombinationPressed;

        #endregion

        #region Public Methods

        public void StartListening(Keys combinationToListen)
        {
            _combinationToListen = combinationToListen;
            _activeCombination = (Keys) 0;

            HookManager.HookManager.KeyDown += HandleGlobalKeyDown;
            HookManager.HookManager.KeyPress += HandleGlobalKeyPress;
            HookManager.HookManager.KeyUp += HandleGlobalKeyUp;
        }

        public void StopListening()
        {
            HookManager.HookManager.KeyDown -= HandleGlobalKeyDown;
            HookManager.HookManager.KeyPress -= HandleGlobalKeyPress;
            HookManager.HookManager.KeyUp -= HandleGlobalKeyUp;
        }

        #endregion

        #region Non Public Methods

        private void HandleGlobalKeyUp(object sender, KeyEventArgs e)
        {
            _activeCombination = EnumUtility.RemoveKey(_activeCombination, e.KeyData);
        }

        private void HandleGlobalKeyPress(object sender, KeyPressEventArgs e)
        {
            //NOTE: HookManager has bugs for handling keys with modifiers and e.KeyChar will be incorrect.
            //Eg. for Ctrl+Shift+M key is 13 (\r); but simply for M key = 'M'.
            //Also, it's impossible to handle modifiers in this handler at all, so we ignore it.
        }

        private void HandleGlobalKeyDown(object sender, KeyEventArgs e)
        {
            _activeCombination = EnumUtility.AddKey(_activeCombination, e.KeyData);

            if (_activeCombination == _combinationToListen)
            {
                e.Handled = true;
                OnKeyCombinationPressed();
            }
        }

        protected virtual void OnKeyCombinationPressed()
        {
            if (KeyCombinationPressed != null)
            {
                KeyCombinationPressed(this, new EventArgs());
            }
        }

        #endregion

    }
}
