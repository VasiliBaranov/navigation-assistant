using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface ISettingsSerializer
    {
        Settings Deserialize();

        ValidationResult Serialize(Settings settings);

        bool GetRunOnStartup();

        void SetRunOnStartup(bool value);
    }
}
