namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Represents a wrapper over a navigator (e.g. Windows Explorer or Total Commander).
    /// </summary>
    public interface INavigator
    {
        void NavigateTo(string path);
    }
}
