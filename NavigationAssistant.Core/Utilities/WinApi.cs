using System;
using System.Runtime.InteropServices;

namespace NavigationAssistant.Core.Utilities
{
    public static class WinApi
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
}
