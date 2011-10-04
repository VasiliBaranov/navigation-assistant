using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface INavigationServiceBuilder
    {
        INavigationService BuildNavigationService(Settings settings);

        void UpdateNavigationSettings(INavigationService navigationService, Settings settings);
    }
}
