using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NavigationAssistant.Core.Model;
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
            bool isInstalling = IsInstalling(e.Args);
            bool isUninstalling = IsUninstalling(e.Args);
            bool isRunOnStartup = IsRunOnStartup(e.Args);

            if (isInstalling)
            {
                Install();
                Shutdown();
                return;
            }

            if (isUninstalling)
            {
                Uninstall();
                Shutdown();
                return;
            }

            PreventDuplicates();
            InitializePresenters(isRunOnStartup);
        }

        private static void Install()
        {
            IRegistryService registryService = new RegistryService();
            IFileSystemParser fileSystemParser = new FileSystemParser(new FileSystemListener());
            ICacheSerializer cacheSerializer = new CacheSerializer();

            //Parse folders and save cache
            List<FileSystemItem> folders = fileSystemParser.GetSubFolders();
            FileSystemCache cache = new FileSystemCache(folders, DateTime.Now);
            cacheSerializer.SerializeCache(cache);

            //Set run on startup = true
            registryService.SetRunOnStartup(true);
        }

        private static void Uninstall()
        {
            IRegistryService registryService = new RegistryService();
            ICacheSerializer cacheSerializer = new CacheSerializer();
            ISettingsSerializer settingsSerializer = new SettingsSerializer(registryService);

            cacheSerializer.DeleteCache();
            registryService.DeleteRunOnStartup();

            //Not sure, if it will be user-friendly; but i prefer if applications remove themselves completely,
            //especially small ones.
            settingsSerializer.DeleteSettings();
        }

        private void PreventDuplicates()
        {
            IPresentationService presentationService = new PresentationService();
            bool appIsRunning = presentationService.ApplicationIsRunning();

            if (appIsRunning)
            {
                MessageBox.Show(NavigationAssistant.Properties.Resources.ProgramIsRunningError,
                                NavigationAssistant.Properties.Resources.ProgramIsRunningErrorCaption);
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
            return HasParameter(parameters, RegistryService.StartupRunParameter);
        }

        private static bool IsInstalling(IEnumerable<string> parameters)
        {
            return HasParameter(parameters, Constants.InstallKey);
        }

        private static bool IsUninstalling(IEnumerable<string> parameters)
        {
            return HasParameter(parameters, Constants.UninstallKey);
        }

        private static bool HasParameter(IEnumerable<string> parameters, string parameterName)
        {
            List<string> list = parameters.ToList();
            return !ListUtility.IsNullOrEmpty(list) && list.Contains(parameterName);
        }

        private void HandleExited(object sender, EventArgs e)
        {
            Shutdown();
        }
    }
}
