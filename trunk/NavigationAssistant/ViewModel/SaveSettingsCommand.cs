using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Core.Model;
using NavigationAssistant.Properties;
using NavigationAssistant.Utilities;

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
            ValidationResult validationResult = _settingsModel.Save();

            if (!validationResult.IsValid)
            {
                ShowErrors(validationResult);
                return;
            }

            Utility.CloseWindow<SettingsWindow>();

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.CurrentNavigationModel.UpdateSettings();
                mainWindow.UpdateIconMenu();
            }

            return;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        private void ShowErrors(ValidationResult validationResult)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("Please correct the following errors:");
            foreach (string errorKey in validationResult.ErrorKeys)
            {
                string error = Resources.ResourceManager.GetString(errorKey, Resources.Culture);
                messageBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "- {0}", error));
            }

            MessageBox.Show(messageBuilder.ToString(), "Errors", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
