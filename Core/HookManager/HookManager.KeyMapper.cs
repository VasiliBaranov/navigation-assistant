using System.Windows.Forms;

namespace Core.HookManager
{
    public static class KeyMapper
    {
        //http://msdn.microsoft.com/en-us/library/windows/desktop/dd375731%28v=vs.85%29.aspx
        //http://msdn.microsoft.com/en-us/library/system.windows.forms.keys.aspx
        public static Keys GetKey(int virtualKeyCode)
        {
            return (Keys) virtualKeyCode;
        }
    }
}
