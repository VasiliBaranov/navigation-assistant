using System.ComponentModel;
using System.Windows.Input;
using NavigationAssistant.PresentationModel;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.PresentationServices.Implementations;

namespace NavigationAssistant.ViewModel
{
    public class SettingsModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly ISettingsSerializer _settingsSerializer;

        private readonly Settings _settings;

        private readonly ICommand _saveCommand;

        private readonly ICommand _cancelCommand;

        private readonly BasicSettingsModel _basicSettings;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public SettingsModel()
        {
            _settingsSerializer = new SettingsSerializer();
            _settings = _settingsSerializer.Deserialize();

            _saveCommand = new SaveSettingsCommand(this);
            _cancelCommand = new CancelSettingsCommand();

            _basicSettings = new BasicSettingsModel(_settings);
        }

        #endregion

        #region Properties

        public BasicSettingsModel BasicSettings
        {
            get
            {
                return _basicSettings;
            }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand; }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        #endregion

        #region Public Methods

        public ValidationResult Save()
        {
            return _settingsSerializer.Serialize(_settings);
        }

        #endregion

        #region Non Public Methods

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
