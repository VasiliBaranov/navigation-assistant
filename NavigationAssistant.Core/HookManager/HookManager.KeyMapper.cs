using System.Windows.Forms;

namespace Core.HookManager
{
    public static class KeyMapper
    {
        /// <summary>
        /// Maps the WinApi virtual key code to the .Net Keys enumeration.
        /// </summary>
        /// <param name="virtualKeyCode">The WinApi virtual key code.</param>
        /// <returns></returns>
        /// <remarks>
        /// For WinApi virtual key codes refer to http://msdn.microsoft.com/en-us/library/windows/desktop/dd375731%28v=vs.85%29.aspx.
        /// For System.Windows.Forms.Keys refer to http://msdn.microsoft.com/en-us/library/system.windows.forms.keys.aspx
        /// </remarks>
        public static Keys GetKey(int virtualKeyCode)
        {
            if (virtualKeyCode == 164)
            {
                return Keys.Alt;
            }

            Keys keyCode = (Keys) virtualKeyCode;
            if (keyCode == Keys.LControlKey || keyCode == Keys.RControlKey)
            {
                return Keys.Control;
            }

            if (keyCode == Keys.LShiftKey || keyCode == Keys.RShiftKey)
            {
                return Keys.Shift;
            }

            return keyCode;
        }
    }
}
