using System;
using System.Windows.Input;

namespace NavigationAssistant.ViewModel
{
    public class SaveSettingsCommand : ICommand
    {
        private readonly SettingsModel _settingsModel;

        public SaveSettingsCommand(SettingsModel settingsModel)
        {
            _settingsModel = settingsModel;
        }

        public void Execute(object parameter)
        {
            _settingsModel.Save();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
