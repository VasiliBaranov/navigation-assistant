using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Views
{
    public interface ISettingsView : IView
    {
        void ShowValidationResult(ValidationResult validationResult);
    }
}
