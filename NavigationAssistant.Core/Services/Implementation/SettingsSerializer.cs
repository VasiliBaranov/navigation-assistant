using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Core.Services.Implementation
{
    /// <summary>
    /// Implements saving and loading user settings.
    /// </summary>
    /// <remarks>
    /// With *.settings files approach app.config become awful.
    /// Custom Configuration Sections approach is weird and too verbose.
    /// So we use an approach from http://msdn.microsoft.com/en-us/library/ms973902.aspx
    /// with custom xml serialization of settings to user folders.
    /// </remarks>
    public class SettingsSerializer : ISettingsSerializer
    {
        #region Fields

        private const string SettingsFileName = "UserSettings.config";

        private static readonly object SettingsSync = new object();

        private readonly IRegistryService _registryService;

        private readonly string _settingsFilePath;

        #endregion

        #region Constructors

        public SettingsSerializer(IRegistryService registryService)
        {
            _registryService = registryService;
            _settingsFilePath = Path.Combine(Application.LocalUserAppDataPath, SettingsFileName);
        }

        public SettingsSerializer(IRegistryService registryService, string settingsFilePath)
        {
            _registryService = registryService;
            _settingsFilePath = settingsFilePath;
        }

        #endregion

        #region Public Methods

        public Settings Deserialize()
        {
            bool settingsAreDefault = !File.Exists(_settingsFilePath);
            Settings settings = settingsAreDefault ? GetDefaultSettings() : DeserializeXml();

            settings.RunOnStartup = GetRunOnStartup();

            ValidateTotalCommanderPath(settings);

            return settings;
        }

        public ValidationResult Serialize(Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            ValidationResult validationResult = ValidateSettings(settings);
            if (validationResult.ErrorKeys.Count > 0)
            {
                return validationResult;
            }

            DirectoryUtility.EnsureFolder(Path.GetDirectoryName(_settingsFilePath));

            XmlSerializer serializer = new XmlSerializer(typeof (Settings));

            lock (SettingsSync)
            {
                using (StreamWriter writer = new StreamWriter(_settingsFilePath, false))
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

        public void DeleteSettings()
        {
            lock (SettingsSync)
            {
                if (File.Exists(_settingsFilePath))
                {
                    File.Delete(_settingsFilePath);
                }

                //Default cache location is c:\Users\<User>\AppData\Local\NavigationAssistant\NavigationAssistant\1.0.0.0\
                //So we have to delete three folders up to Local.
                string folderPath = Path.GetDirectoryName(_settingsFilePath);
                DirectoryUtility.DeleteUpperFolder(folderPath, 2);
            }
        }

        #endregion

        #region Non Public Methods

        private Settings DeserializeXml()
        {
            Settings settings;
            lock (SettingsSync)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    using (StreamReader reader = new StreamReader(_settingsFilePath))
                    {
                        settings = serializer.Deserialize(reader) as Settings;
                    }
                }
                catch (Exception)
                {
                    settings = GetDefaultSettings();
                }
            }
            return settings;
        }

        private static ValidationResult ValidateSettings(Settings settings)
        {
            List<string> errorKeys = new List<string>();

            //Check total commander
            bool totalCommanderSupported = settings.SupportedNavigators != null &&
                                           settings.SupportedNavigators.Contains(Navigators.TotalCommander);

            bool totalCommanderPathValid = !string.IsNullOrEmpty(settings.TotalCommanderPath) &&
                                           File.Exists(settings.TotalCommanderPath);

            if (totalCommanderSupported && !totalCommanderPathValid)
            {
                errorKeys.Add("TotalCommanderPathInvalidError");
            }

            //Check exclude path regexes
            bool excludeRegexesValid = true;
            if (settings.ExcludeFolderTemplates != null)
            {
                foreach (string excludeFolderTemplate in settings.ExcludeFolderTemplates)
                {
                    try
                    {
                        new Regex(excludeFolderTemplate);
                    }
                    catch
                    {
                        excludeRegexesValid = false;
                    }
                }
            }

            if (!excludeRegexesValid)
            {
                errorKeys.Add("ExcludeFolderTemplatesInvalidError");
            }

            return new ValidationResult(errorKeys);
        }

        //This check is needed, as Total Commander may be deleted or moved since the last settings save.
        //Also, registry may contain an outdated value.
        private void ValidateTotalCommanderPath(Settings settings)
        {
            if (string.IsNullOrEmpty(settings.TotalCommanderPath) || !File.Exists(settings.TotalCommanderPath))
            {
                settings.TotalCommanderPath = GetTotalCommanderPath();
            }

            if (string.IsNullOrEmpty(settings.TotalCommanderPath))
            {
                settings.SupportedNavigators = new List<Navigators> {Navigators.WindowsExplorer};
                settings.PrimaryNavigator = Navigators.WindowsExplorer;
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
