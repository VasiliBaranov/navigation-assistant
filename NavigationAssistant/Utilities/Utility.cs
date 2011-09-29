using System.Linq;
using System.Windows;

namespace NavigationAssistant.Utilities
{
    public static class Utility
    {
        public static void CloseWindow<T>() where T : Window
        {
            T settingsWindow = Application.Current.Windows.OfType<T>().FirstOrDefault();
            if (settingsWindow != null && settingsWindow.IsActive)
            {
                settingsWindow.Close();
            }
        }
    }
}
