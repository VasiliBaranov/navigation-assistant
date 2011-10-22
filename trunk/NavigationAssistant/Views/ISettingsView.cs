using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Views
{
    /// <summary>
    /// Defines methods for the settings window.
    /// </summary>
    public interface ISettingsView : IView
    {
        void ShowValidationResult(ValidationResult validationResult);
    }
}
