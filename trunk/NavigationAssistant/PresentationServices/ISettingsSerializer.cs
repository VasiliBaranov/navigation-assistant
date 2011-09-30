using NavigationAssistant.PresentationModel;

namespace NavigationAssistant.PresentationServices
{
    public interface ISettingsSerializer
    {
        Settings Deserialize();

        ValidationResult Serialize(Settings settings);

        bool GetRunOnStartup();

        void SetRunOnStartup(bool value);
    }
}
