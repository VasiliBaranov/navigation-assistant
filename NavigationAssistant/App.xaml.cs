using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using NLog;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.Core.Utilities;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.PresentationServices.Implementations;
using NavigationAssistant.Presenters;
using NavigationAssistant.Presenters.Implementation;
using NavigationAssistant.Views.Implementation;

namespace NavigationAssistant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IPresenterManager _presenterManager;

        private static readonly Logger CurrentLogger = LogManager.GetCurrentClassLogger();

        private static readonly Mutex AppMutex = new Mutex(true, "44F16610-EF84-47B6-8536-33C0D754F41E");

        private void HandleApplicationStartup(object sender, StartupEventArgs e)
        {
            //See http://stackoverflow.com/questions/1472498/wpf-global-exception-handler
            AppDomain.CurrentDomain.UnhandledException += HandleGlobalException;

            string args = string.Join("; ", e.Args);
            CurrentLogger.Info("Launching with console parameters: '{0}'", args);

            bool isRunOnStartup = IsRunOnStartup(e.Args);

            bool shouldContinue = InstallSafe(e);
            if (!shouldContinue)
            {
                return;
            }

            shouldContinue = UninstallSafe(e);
            if (!shouldContinue)
            {
                return;
            }

            if (HasDuplicates())
            {
                CurrentLogger.Info("Closing application as duplicate");
                Shutdown();
            }
            else
            {
                InitializePresenters(isRunOnStartup);
            }
        }

        private bool InstallSafe(StartupEventArgs e)
        {
            if (IsInstalling(e.Args))
            {
                try
                {
                    Install();
                }
                catch (Exception ex)
                {
                    //Should not throw errors in InnoSetup.
                    CurrentLogger.ErrorException("Exception during install", ex);
                }

                Shutdown();
                return false;
            }

            return true;
        }

        private bool UninstallSafe(StartupEventArgs e)
        {
            if (IsUninstalling(e.Args))
            {
                try
                {
                    Uninstall();
                }
                catch (Exception ex)
                {
                    //Should not throw errors in InnoSetup.
                    CurrentLogger.ErrorException("Exception during uninstall", ex);
                }

                Shutdown();
                return false;
            }

            return true;
        }

        private static void HandleGlobalException(object sender, UnhandledExceptionEventArgs e)
        {
            CurrentLogger.ErrorException("Unhandled exception", e.ExceptionObject as Exception);
        }

        private static void Install()
        {
            IRegistryService registryService = new RegistryService();

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

        private bool HasDuplicates()
        {
            IPresentationService presentationService = new PresentationService();
            bool hasDuplicates = presentationService.ApplicationIsRunning(AppMutex);

            if (hasDuplicates)
            {
                MessageBox.Show(NavigationAssistant.Properties.Resources.ProgramIsRunningError,
                                NavigationAssistant.Properties.Resources.ProgramIsRunningErrorCaption);
            }
            return hasDuplicates;
        }

        //NOTE: Don't use IoC to avoid additional dependencies.
        private void InitializePresenters(bool isRunOnStartup)
        {
            NavigationWindow navigationWindow = new NavigationWindow(new PresentationService());
            MainWindow = navigationWindow;

            IRegistryService registryService = new RegistryService();
            ISettingsSerializer settingsSerializer = new SettingsSerializer(registryService);

            IPresenter navigationPresenter = new NavigationPresenter(navigationWindow,
                                     settingsSerializer,
                                     new KeyboardListener(),
                                     new MatchModelMapper(),
                                     new PresentationService(),
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
            if (string.IsNullOrEmpty(parameterName) || parameters == null)
            {
                return false;
            }

            List<string> list = parameters.ToList();
            return !ListUtility.IsNullOrEmpty(list) && list.Contains(parameterName);
        }

        private void HandleExited(object sender, EventArgs e)
        {
            CurrentLogger.Info("Shutting down");
            Shutdown();
        }
    }
}
