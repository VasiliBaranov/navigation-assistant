using System.IO;
using SHDocVw;

namespace Core.Services.Implementation
{
    public class WindowsExplorer : IExplorer
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
