using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Core.Utilities;

namespace Core.Services.Implementation
{
    public class KeyboardListener : IKeyboardListener
    {
        #region Fields

        private KeyCombination _combinationToListen;

        private KeyCombination _activeCombination;

        public event EventHandler KeyCombinationPressed;

        #endregion

        #region Public Methods

        public void StartListening(Keys combinationToListen)
        {
            _combinationToListen = BuildCombination(combinationToListen);
            _activeCombination = new KeyCombination();

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

        private static KeyCombination BuildCombination(Keys key)
        {
            KeyCombination keyCombination = new KeyCombination();
            if (EnumUtility.IsPresent(key, Keys.Control))
            {
                keyCombination.Add(KeyGroup.Control);
            }
            if (EnumUtility.IsPresent(key, Keys.Shift))
            {
                keyCombination.Add(KeyGroup.Shift);
            }
            if (EnumUtility.IsPresent(key, Keys.Alt))
            {
                keyCombination.Add(KeyGroup.Alt);
            }

            Keys notModifiedKey = EnumUtility.ExtractNotModifiedKey(key);
            keyCombination.Add(new KeyGroup(notModifiedKey));

            return keyCombination;
        }

        private static KeyGroup GetKeyGroup(KeyCombination keyCombination, Keys key)
        {
            foreach (KeyGroup keyEquivalents in keyCombination)
            {
                foreach (Keys currentKey in keyEquivalents)
                {
                    if (currentKey == key)
                    {
                        return keyEquivalents;
                    }
                }
            }

            return null;
        }

        private void HandleGlobalKeyUp(object sender, KeyEventArgs e)
        {
            KeyGroup pressedKeyGroup = GetKeyGroup(_combinationToListen, e.KeyData);

            //Active combination may contain the pressed key group, if (e.g.) left and right controls are pressed.
            if (pressedKeyGroup != null && _activeCombination.Contains(pressedKeyGroup))
            {
                _activeCombination.Remove(pressedKeyGroup);
            }
        }

        private void HandleGlobalKeyPress(object sender, KeyPressEventArgs e)
        {
            //NOTE: HookManager has bugs for handling keys with modifiers and e.KeyChar will be incorrect.
            //Eg. for Ctrl+Shift+M key is 13 (\r); but simply for M key = 'M'.
            //Also, it's impossible to handle modifiers in this handler at all, so we ignore it.
        }

        private void HandleGlobalKeyDown(object sender, KeyEventArgs e)
        {
            KeyGroup pressedKeyGroup = GetKeyGroup(_combinationToListen, e.KeyData);

            //Active combination may contain the pressed key group, if (e.g.) left and right controls are pressed.
            if (pressedKeyGroup != null && !_activeCombination.Contains(pressedKeyGroup))
            {
                _activeCombination.Add(pressedKeyGroup);
            }

            bool correctCombinationClicked = (_activeCombination.Count == _combinationToListen.Count);
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

        #endregion

        #region Nested Classes

        private class KeyCombination : List<KeyGroup>
        {

        }

        //HookManager returns Keys.LControlKey if left control is used (which is correct, btw);
        //but Keys.LControlKey == RButton | Space | F17 (god knows, why).
        //So it's impossible to use Keys enum to determine that a control has been clicked,
        //that's why these classes are used.
        private class KeyGroup : List<Keys>
        {
            public KeyGroup()
            {

            }

            public KeyGroup(Keys key)
            {
                Add(key);
            }

            public static KeyGroup Control
            {
                get { return new KeyGroup { Keys.LControlKey, Keys.RControlKey }; }
            }

            public static KeyGroup Shift
            {
                get { return new KeyGroup { Keys.LShiftKey, Keys.RShiftKey }; }
            }

            public static KeyGroup Alt
            {
                get { return new KeyGroup(Keys.Alt); }
            }
        }

        #endregion

    }
}
