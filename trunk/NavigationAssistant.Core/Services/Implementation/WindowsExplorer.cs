using System.IO;
using SHDocVw;

namespace NavigationAssistant.Core.Services.Implementation
{
    /// <summary>
    /// Implements a wrapper over Windows Explorer.
    /// </summary>
    public class WindowsExplorer : INavigator
    {
        private readonly InternetExplorer _windowsExplorer;

        public WindowsExplorer(InternetExplorer windowsExplorer)
        {
            _windowsExplorer = windowsExplorer;
        }

        public void NavigateTo(string path)
        {
            try
            {
                string fullPath = Path.GetFullPath(path);
                _windowsExplorer.Navigate("file:///" + fullPath);
            }
            catch (PathTooLongException)
            {
                //NOTE: may be escalate the exception?
            }
        }
    }
}
