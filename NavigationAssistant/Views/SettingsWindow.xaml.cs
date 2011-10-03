using System;
using System.Globalization;
using System.Text;
using System.Windows;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.Presenters;
using NavigationAssistant.ViewModel;

namespace NavigationAssistant.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : ISettingsView
    {
        #region Fields

        public event EventHandler SettingsChanged;

        private readonly SettingsModel _viewModel;

        private readonly SettingsPresenter _settingsPresenter;

        #endregion

        #region Constructors

        public SettingsWindow()
        {
            InitializeComponent();
            _viewModel = new SettingsModel();

            _settingsPresenter = new SettingsPresenter(this, new SettingsSerializer());

            DataContext = _viewModel;
        }

        #endregion

        #region Properties

        public Settings CurrentSettings
        {
            get
            {
                return _viewModel.Settings;
            }
            set
            {
                _viewModel.Settings = value;
            }
        }

        #endregion

        #region Public Methods

        public void ShowValidationResult(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                ShowErrors(validationResult);
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void ShowView()
        {
            Show();
        }

        public void HideView()
        {
            Hide();
        }

        #endregion

        #region Non Public Methods

        private static void ShowErrors(ValidationResult validationResult)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("Please correct the following errors:");
            foreach (string errorKey in validationResult.ErrorKeys)
            {
                string error = Properties.Resources.ResourceManager.GetString(errorKey, Properties.Resources.Culture);
                messageBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "- {0}", error));
            }

            MessageBox.Show(messageBuilder.ToString(), "Errors", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void FireEvent(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void HandleSaveButtonClick(object sender, RoutedEventArgs e)
        {
            FireEvent(SettingsChanged);
        }

        private void HandleCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        #endregion
    }
}
