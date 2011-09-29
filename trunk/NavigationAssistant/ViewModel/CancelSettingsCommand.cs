using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NavigationAssistant.ViewModel
{
    public class CancelSettingsCommand : ICommand
    {
        public void Execute(object parameter)
        {
            SettingsWindow settingsWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
            if (settingsWindow != null && settingsWindow.IsActive)
            {
                settingsWindow.Close();
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
