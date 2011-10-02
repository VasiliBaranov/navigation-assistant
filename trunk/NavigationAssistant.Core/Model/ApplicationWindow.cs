using System;

namespace NavigationAssistant.Core.Model
{
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
