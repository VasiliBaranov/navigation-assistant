using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.Core.Utilities;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.PresentationServices.Implementations;
using NavigationAssistant.Presenters;
using NavigationAssistant.Views.Implementation;

namespace NavigationAssistant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IPresenterManager _presenterManager;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool isRunOnStartup = IsRunOnStartup(e.Args);

            PreventDuplicates();
            InitializePresenters(isRunOnStartup);
        }

        private void PreventDuplicates()
        {
            IPresentationService presentationService = new PresentationService();
            bool appIsRunning = presentationService.ApplicationIsRunning();

            if (appIsRunning)
            {
                MessageBox.Show(NavigationAssistant.Properties.Resources.ProgramIsRunningError);
                Shutdown();
            }
        }

        private void InitializePresenters(bool isRunOnStartup)
        {
            NavigationWindow navigationWindow = new NavigationWindow();
            MainWindow = navigationWindow;

            IRegistryService registryService = new RegistryService();
            ISettingsSerializer settingsSerializer = new SettingsSerializer(registryService);

            IPresenter navigationPresenter = new NavigationPresenter(navigationWindow,
                                     settingsSerializer,
                                     new KeyboardListener(),
                                     new MatchModelMapper(),
                                     new NavigationServiceBuilder(isRunOnStartup));

            TrayView trayView = new TrayView();
            IPresenter trayPresenter = new TrayIconPresenter(trayView, settingsSerializer);

            SettingsWindow settingsWindow = new SettingsWindow();
            IPresenter settingsPresenter = new SettingsPresenter(settingsWindow, settingsSerializer);

            List<IPresenter> presenters = new List<IPresenter> { navigationPresenter, trayPresenter, settingsPresenter };

            _presenterManager = new PresenterManager(presenters);
            _presenterManager.Exited += HandleExited;
        }

        private static bool IsRunOnStartup(IEnumerable<string> parameters)
        {
            List<string> list = parameters.ToList();
            return !ListUtility.IsNullOrEmpty(list) && list.Contains(RegistryService.StartupRunParameter);
        }

        private void HandleExited(object sender, System.EventArgs e)
        {
            Shutdown();
        }
    }
}
