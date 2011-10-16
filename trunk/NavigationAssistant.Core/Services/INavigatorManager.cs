using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines methods for managing navigators of a certain type (e.g. Windows Explorer or Total Commander).
    /// </summary>
    public interface INavigatorManager
    {
        bool IsNavigator(ApplicationWindow hostWindow);

        INavigator GetNavigator(ApplicationWindow hostWindow);

        INavigator CreateNavigator();
    }
}
