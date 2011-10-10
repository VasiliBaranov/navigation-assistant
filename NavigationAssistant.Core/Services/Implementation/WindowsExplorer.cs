using System.IO;
using SHDocVw;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class WindowsExplorer : INavigator
    {
        private readonly InternetExplorer _windowsExplorer;

        public WindowsExplorer(InternetExplorer windowsExplorer)
        {
            _windowsExplorer = windowsExplorer;
        }

        public void NavigateTo(string path)
        {
            _windowsExplorer.Navigate("file:///" + Path.GetFullPath(path));
        }
    }
}
