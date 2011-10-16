using System;

namespace NavigationAssistant.Core.Model
{
    /// <summary>
    /// Represents a wrapper over Win32 window.
    /// </summary>
    public class ApplicationWindow
    {
        public IntPtr Handle { get; set; }

        public ApplicationWindow()
        {
        }

        public ApplicationWindow(IntPtr handle)
        {
            Handle = handle;
        }
    }
}
