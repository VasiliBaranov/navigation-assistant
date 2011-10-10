using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    public interface INavigatorManager
    {
        bool IsNavigator(ApplicationWindow hostWindow);

        INavigator GetNavigator(ApplicationWindow hostWindow);

        INavigator CreateNavigator();
    }
}
