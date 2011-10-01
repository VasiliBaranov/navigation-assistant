using System;
using System.Windows.Forms;
using Core.Model;

namespace Core.Services.Implementation
{
    public class KeyboardListener : IKeyboardListener
    {
        private bool _isCtrlPressed;

        private bool _isShiftPressed;

        public event EventHandler KeyCombinationPressed;

        public void StartListening(KeyCombination combinationToListen)
        {
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

        private void HandleGlobalKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LControlKey)
            {
                _isCtrlPressed = false;
            }

            if (e.KeyCode == Keys.LShiftKey)
            {
                _isShiftPressed = false;
            }
        }

        private void HandleGlobalKeyPress(object sender, KeyPressEventArgs e)
        {
            //NOTE: May be implement setting e.Handled in all global handlers?
        }

        private void HandleGlobalKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LControlKey)
            {
                _isCtrlPressed = true;
            }

            if (e.KeyCode == Keys.LShiftKey)
            {
                _isShiftPressed = true;
            }

            bool correctCombinationClicked = _isCtrlPressed && _isShiftPressed && e.KeyCode == Keys.M;
            if (correctCombinationClicked)
            {
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
    }
}
