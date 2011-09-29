using NavigationAssistant.PresentationModel;

namespace NavigationAssistant.PresentationServices
{
    public interface ISettingsSerializer
    {
        Settings Deserialize();

        ValidationResult Serialize(Settings settings);
    }
}
