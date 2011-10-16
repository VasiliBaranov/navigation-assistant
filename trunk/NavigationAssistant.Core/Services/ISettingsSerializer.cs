using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines methods for saving and loading user settings.
    /// </summary>
    public interface ISettingsSerializer
    {
        Settings Deserialize();

        ValidationResult Serialize(Settings settings);

        bool GetRunOnStartup();

        void SetRunOnStartup(bool value);

        void DeleteSettings();
    }
}
