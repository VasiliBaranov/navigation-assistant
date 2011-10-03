using System;
using System.Windows.Input;
using NavigationAssistant.Utilities;
using NavigationAssistant.Views;

namespace NavigationAssistant.ViewModel
{
    public class CancelSettingsCommand : ICommand
    {
        public void Execute(object parameter)
        {
            Utility.CloseWindow<SettingsWindow>();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
