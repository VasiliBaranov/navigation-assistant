using NavigationAssistant.Core.Model;

namespace NavigationAssistant.ViewModel
{
    /// <summary>
    /// Implements a settings view model.
    /// </summary>
    public class SettingsModel : BaseViewModel
    {
        #region Fields

        private Settings _settings;

        private readonly BasicSettingsModel _basicSettings = new BasicSettingsModel();

        private readonly AdvancedSettingsModel _advancedSettings = new AdvancedSettingsModel();

        #endregion

        #region Constructors

        #endregion

        #region Properties

        public Settings Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
                _basicSettings.Settings = value;
                _advancedSettings.Settings = value;

                OnPropertyChanged("BasicSettings");
                OnPropertyChanged("AdvancedSettings");
            }
        }

        public BasicSettingsModel BasicSettings
        {
            get
            {
                return _basicSettings;
            }
        }

        public AdvancedSettingsModel AdvancedSettings
        {
            get
            {
                return _advancedSettings;
            }
        }

        #endregion
    }
}
