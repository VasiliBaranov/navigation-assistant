using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    //With *.settings files approach app.config become awful.
    //Custom Configuration Sections approach is weird and too verbose.
    //So we use an approach from http://msdn.microsoft.com/en-us/library/ms973902.aspx
    //with custom xml serialization of settings to user folders.
    public class SettingsSerializer : ISettingsSerializer
    {
        private const string SettingsFileName = "UserSettings.config";

        private static readonly object SettingsSync = new object();

        private readonly IRegistryService _registryService;

        public SettingsSerializer(IRegistryService registryService)
        {
            _registryService = registryService;
        }

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
                lock (SettingsSync)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    using (StreamReader reader = new StreamReader(settingsFileName))
                    {
                        settings = serializer.Deserialize(reader) as Settings;
                    }
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

            lock (SettingsSync)
            {
                using (StreamWriter writer = new StreamWriter(settingsFileName, false))
                {
                    serializer.Serialize(writer, settings);
                }
            }

            SetRunOnStartup(settings.RunOnStartup);

            return validationResult;
        }

        public bool GetRunOnStartup()
        {
            return _registryService.GetRunOnStartup();
        }

        public void SetRunOnStartup(bool value)
        {
            _registryService.SetRunOnStartup(value);
        }

        #endregion

        #region Non Public Methods

        private static ValidationResult ValidateSettings(Settings settings)
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

        private static string GetSettingsFileName(bool ensureFolder)
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
                                        ExcludeFolderTemplates = new List<string> {"obj", "bin", ".svn"},
                                        FoldersToParse = null,
                                        PrimaryNavigator = Navigators.WindowsExplorer,
                                        TotalCommanderPath = GetTotalCommanderPath(),
                                        GlobalKeyCombination = Keys.Control | Keys.Shift | Keys.M
                                    };

            return settings;
        }

        private string GetTotalCommanderPath()
        {
            string folder = _registryService.GetTotalCommanderFolder();
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

        #endregion
    }
}
