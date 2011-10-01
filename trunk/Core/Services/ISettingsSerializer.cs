using Core.Model;

namespace Core.Services
{
    public interface ISettingsSerializer
    {
        Settings Deserialize();

        ValidationResult Serialize(Settings settings);

        bool GetRunOnStartup();

        void SetRunOnStartup(bool value);

        INavigationService BuildNavigationService(Settings settings);
    }
}
