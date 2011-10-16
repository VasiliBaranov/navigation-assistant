using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines methods for building and updating navigation service according to settings provided.
    /// </summary>
    public interface INavigationServiceBuilder
    {
        INavigationService BuildNavigationService(Settings settings);

        void UpdateNavigationSettings(INavigationService navigationService, Settings settings);
    }
}
