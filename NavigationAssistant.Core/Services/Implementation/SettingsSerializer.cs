using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;
using Microsoft.Win32;

namespace NavigationAssistant.Core.Services.Implementation
{
    //With *.settings files approach app.config become awful.
    //Custom Configuration Sections approach is weird and too verbose.
    //So we use an approach from http://msdn.microsoft.com/en-us/library/ms973902.aspx
    //with custom xml serialization of settings to user folders.
    public class SettingsSerializer : ISettingsSerializer
    {
        private const string SettingsFileName = "UserSettings.config";

        #region Public Methods

        public Settings Deserialize()
        {
            string settingsFileName = GetSettingsFileName(false);

            Settings settings;
            bool settingsAreDefault = !File.Exists(settingsFileName);
            if (settingsAreDefault)
            {
                settings = GetDefaultSettings();
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                using (StreamReader reader = new StreamReader(settingsFileName))
                {
                    settings = serializer.Deserialize(reader) as Settings;
                }
            }

            settings.RunOnStartup = GetRunOnStartup();

            ValidateTotalCommanderPath(settings, settingsAreDefault);

            return settings;
        }

        public ValidationResult Serialize(Settings settings)
        {
            ValidationResult validationResult = ValidateSettings(settings);
            if (validationResult.ErrorKeys.Count > 0)
            {
                return validationResult;
            }

            string settingsFileName = GetSettingsFileName(true);

            XmlSerializer serializer = new XmlSerializer(typeof (Settings));

            using (StreamWriter writer = new StreamWriter(settingsFileName, false))
            {
                serializer.Serialize(writer, settings);
            }

             SetRunOnStartup(settings.RunOnStartup);

            return validationResult;
        }

        public bool GetRunOnStartup()
        {
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            string navigationAssistantPath = startupKey.GetValue("NavigationAssistant") as string;

            bool runOnStartup = navigationAssistantPath != null;
            return runOnStartup;
        }

        public void SetRunOnStartup(bool value)
        {
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

            if (value)
            {
                string path = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", Assembly.GetEntryAssembly().Location);
                startupKey.SetValue("NavigationAssistant", path, RegistryValueKind.String);
            }
            else
            {
                bool valueExists = GetRunOnStartup();
                if (valueExists)
                {
                    startupKey.DeleteValue("NavigationAssistant");
                }
            }
        }

        public INavigationService BuildNavigationService(Settings settings)
        {
            //NOTE: We turn off listening for attributes changes, as it may be too slow;
            //and we also do not display IncludeHiddenFolders option in the UI. So attributes are never used.
            IFileSystemParser basicParser = new FileSystemParser(new FileSystemListener(false));
            ICacheSerializer cacheSerializer = new CacheSerializer(settings.CacheFolder);
            IFileSystemParser cachedParser = new CachedFileSystemParser(basicParser, cacheSerializer, new FileSystemListener(false),
                settings.CacheUpdateDelayInSeconds);
            cachedParser.IncludeHiddenFolders = settings.IncludeHiddenFolders;
            cachedParser.ExcludeFolderTemplates = settings.ExcludeFolderTemplates;
            cachedParser.FoldersToParse = settings.FoldersToParse;

            List<Navigators> additionalNavigators = new List<Navigators>(settings.SupportedNavigators);
            additionalNavigators.Remove(settings.PrimaryNavigator);

            List<IExplorerManager> supportedExplorerManagers =
                additionalNavigators
                    .Select(navigator => CreateExplorerManager(navigator, settings))
                    .ToList();

            IExplorerManager primaryExplorerManager = CreateExplorerManager(settings.PrimaryNavigator, settings);
            supportedExplorerManagers.Add(primaryExplorerManager);

            INavigationService navigationAssistant = new NavigationService(cachedParser, new MatchSearcher(), primaryExplorerManager, supportedExplorerManagers);

            //Warming up (to fill caches, etc)
            navigationAssistant.GetFolderMatches("temp");

            return navigationAssistant;
        }

        #endregion

        #region Non Public Methods

        private IExplorerManager CreateExplorerManager(Navigators navigator, Settings settings)
        {
            if (navigator == Navigators.TotalCommander)
            {
                return new TotalCommanderManager(settings.TotalCommanderPath);
            }
            else
            {
                return new WindowsExplorerManager();
            }
        }

        private ValidationResult ValidateSettings(Settings settings)
        {
            List<string> errorKeys = new List<string>();

            bool totalCommanderSupported = settings.SupportedNavigators.Contains(Navigators.TotalCommander);
            bool totalCommanderPathValid = !string.IsNullOrEmpty(settings.TotalCommanderPath) &&
                                           File.Exists(settings.TotalCommanderPath);

            if (totalCommanderSupported && !totalCommanderPathValid)
            {
                errorKeys.Add("TotalCommanderPathInvalidError");
            }

            return new ValidationResult(errorKeys);
        }

        private string GetSettingsFileName(bool ensureFolder)
        {
            string settingsFolder = Application.LocalUserAppDataPath;
            if (ensureFolder)
            {
                DirectoryUtility.EnsureFolder(settingsFolder);
            }

            return Path.Combine(settingsFolder, SettingsFileName);
        }

        //This check is needed, as Total Commander may be deleted or moved since the last settings save.
        //Also, registry may contain an outdated value.
        private void ValidateTotalCommanderPath(Settings settings, bool settingsAreDefault)
        {
            if (string.IsNullOrEmpty(settings.TotalCommanderPath))
            {
                return;
            }

            if (!File.Exists(settings.TotalCommanderPath))
            {
                string fallbackValue = settingsAreDefault ? null : GetTotalCommanderPath();
                settings.TotalCommanderPath = fallbackValue;
            }
        }

        private Settings GetDefaultSettings()
        {
            Settings settings = new Settings
                                    {
                                        SupportedNavigators = new List<Navigators> {Navigators.TotalCommander, Navigators.WindowsExplorer},
                                        CacheFolder = Application.CommonAppDataPath,
                                        CacheUpdateDelayInSeconds = 60*10,
                                        ExcludeFolderTemplates = new List<string> {"obj", "bin", ".svn"},
                                        FoldersToParse = null,
                                        IncludeHiddenFolders = true,
                                        PrimaryNavigator = Navigators.WindowsExplorer,
                                        TotalCommanderPath = GetTotalCommanderPath(),
                                        GlobalKeyCombination = Keys.Control | Keys.Shift | Keys.M
                                    };

            return settings;
        }

        private string GetTotalCommanderPath()
        {
            string folder = GetTotalCommanderFolder();
            if (string.IsNullOrEmpty(folder))
            {
                return null;
            }

            string path = Path.Combine(folder, "TOTALCMD.EXE");
            if (!File.Exists(path))
            {
                return null;
            }

            return path;
        }

        private string GetTotalCommanderFolder()
        {
            List<RegistryKey> registryKeys = new List<RegistryKey> {Registry.LocalMachine, Registry.CurrentUser};

            foreach (RegistryKey registryKey in registryKeys)
            {
                try
                {
                    RegistryKey installDirValue = registryKey.OpenSubKey(@"Software\Ghisler\Total Commander");

                    if (installDirValue != null)
                    {
                        string installDir = installDirValue.GetValue("InstallDir") as string;
                        if (!string.IsNullOrEmpty(installDir) && Directory.Exists(installDir))
                        {
                            return installDir;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        #endregion
    }
}
