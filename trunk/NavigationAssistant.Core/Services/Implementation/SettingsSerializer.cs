using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        private static readonly object SettingsSync = new object();

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
