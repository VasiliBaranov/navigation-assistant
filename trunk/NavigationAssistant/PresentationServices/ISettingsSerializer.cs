using NavigationAssistant.PresentationModel;

namespace NavigationAssistant.PresentationServices
{
    public interface ISettingsSerializer
    {
        Settings Deserialize();

        void Serialize(Settings settings);
    }
}
