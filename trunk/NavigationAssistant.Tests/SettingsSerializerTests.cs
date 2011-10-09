using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class SettingsSerializerTests
    {
        private FakeRegistryService _registryService;
        private ISettingsSerializer _settingsSerializer;
        private const string TempFolder = "Temp";
        private const string SettingsFilePath = TempFolder + "\\Settings\\Settings.config";
        private const string TotalCommanderPath = TempFolder + "\\TotalCommander.txt";

        [SetUp]
        public void SetUp()
        {
            _registryService = new FakeRegistryService();
            _settingsSerializer = new SettingsSerializer(_registryService, SettingsFilePath);

            DirectoryUtility.EnsureClearFolder(TempFolder);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(TempFolder, true);
        }

        [Test]
        public void Serialize_FolderAndFileDontExist_FolderAndFileCreated()
        {
            Settings settings = new Settings { SupportedNavigators = new List<Navigators>() };
            ValidationResult result = _settingsSerializer.Serialize(settings);

            Assert.That(File.Exists(SettingsFilePath), Is.True);
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void Serialize_SupportTotalCommanderTotalCommanderDoesntExist_ReturnValidationError()
        {
            Settings settings = new Settings
                                    {
                                        SupportedNavigators = new List<Navigators> {Navigators.TotalCommander},
                                        TotalCommanderPath = TotalCommanderPath
                                    };
            ValidationResult result = _settingsSerializer.Serialize(settings);

            Assert.That(result.IsValid, Is.False);
            Assert.That(File.Exists(SettingsFilePath), Is.False);
        }

        [Test]
        public void Serialize_SupportTotalCommanderTotalCommanderEmpty_ReturnValidationError()
        {
            Settings settings = new Settings
                                    {
                                        SupportedNavigators = new List<Navigators> {Navigators.TotalCommander}
                                    };
            ValidationResult result = _settingsSerializer.Serialize(settings);

            Assert.That(result.IsValid, Is.False);
            Assert.That(File.Exists(SettingsFilePath), Is.False);
        }

        [Test]
        public void Serialize_DontSupportTotalCommanderTotalCommanderEmpty_SaveCorrectly()
        {
            Settings settings = new Settings { SupportedNavigators = new List<Navigators>() };
            ValidationResult result = _settingsSerializer.Serialize(settings);

            Assert.That(File.Exists(SettingsFilePath), Is.True);
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void Serialize_DontSupportTotalCommanderTotalCommanderDoesntExist_SaveCorrectly()
        {
            Settings settings = new Settings
                                    {
                                        SupportedNavigators = new List<Navigators>(),
                                        TotalCommanderPath = TotalCommanderPath
                                    };
            ValidationResult result = _settingsSerializer.Serialize(settings);

            Assert.That(File.Exists(SettingsFilePath), Is.True);
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void Serialize_SupportTotalCommanderTotalCommanderExists_SaveCorrectly()
        {
            File.WriteAllText(TotalCommanderPath, string.Empty);
            Settings settings = new Settings
                                    {
                                        SupportedNavigators = new List<Navigators> { Navigators.TotalCommander },
                                        TotalCommanderPath = TotalCommanderPath
                                    };
            ValidationResult result = _settingsSerializer.Serialize(settings);

            Assert.That(File.Exists(SettingsFilePath), Is.True);
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void Deserialize_AfterSerialization_SettingsCorrect()
        {
            File.WriteAllText(TotalCommanderPath, string.Empty);
            Settings expectedSettings = new Settings
                                            {
                                                PrimaryNavigator = Navigators.TotalCommander,
                                                SupportedNavigators = new List<Navigators> {Navigators.TotalCommander},
                                                TotalCommanderPath = TotalCommanderPath,
                                                FoldersToParse = new List<string> {"D:\\temp"},
                                                ExcludeFolderTemplates = new List<string> {"obj", "svn"},
                                                GlobalKeyCombination = Keys.Shift | Keys.M
                                            };
            _settingsSerializer.Serialize(expectedSettings);

            Settings actualSettings = _settingsSerializer.Deserialize();

            Assert.That(actualSettings.PrimaryNavigator, Is.EqualTo(expectedSettings.PrimaryNavigator));
            Assert.That(actualSettings.SupportedNavigators, Is.EquivalentTo(expectedSettings.SupportedNavigators));
            Assert.That(actualSettings.TotalCommanderPath, Is.EqualTo(expectedSettings.TotalCommanderPath));
            Assert.That(actualSettings.FoldersToParse, Is.EquivalentTo(expectedSettings.FoldersToParse));
            Assert.That(actualSettings.ExcludeFolderTemplates, Is.EquivalentTo(expectedSettings.ExcludeFolderTemplates));
            Assert.That(actualSettings.GlobalKeyCombination, Is.EqualTo(expectedSettings.GlobalKeyCombination));
        }

        [Test]
        public void Deserialize_FolderAndFileDontExist_ReturnDefaultSettings()
        {
            Settings actualSettings = _settingsSerializer.Deserialize();

            Assert.That(actualSettings.GlobalKeyCombination, Is.EqualTo(Keys.Control | Keys.Shift | Keys.M));
        }

        [Test]
        public void Deserialize_TotalCommanderDoesntExistRegistryPathExists_ReturnPathFromRegistry()
        {
            File.WriteAllText(TotalCommanderPath, string.Empty);
            Settings expectedSettings = new Settings
            {
                TotalCommanderPath = TotalCommanderPath
            };
            _settingsSerializer.Serialize(expectedSettings);

            File.Delete(TotalCommanderPath);

            const string currentTotalCmdFolder = TempFolder + "\\TotalCmd";
            const string currentTotalCmdPath = currentTotalCmdFolder + "\\TOTALCMD.EXE";
            _registryService.TotalCommanderFolder = currentTotalCmdFolder;

            DirectoryUtility.EnsureClearFolder(currentTotalCmdFolder);
            File.WriteAllText(currentTotalCmdPath, string.Empty);

            Settings actualSettings = _settingsSerializer.Deserialize();

            Assert.That(actualSettings.TotalCommanderPath, Is.EqualTo(currentTotalCmdPath));
        }

        [Test]
        public void Deserialize_TotalCommanderEmptyRegistryPathExists_ReturnPathFromRegistry()
        {
            Settings expectedSettings = new Settings();
            _settingsSerializer.Serialize(expectedSettings);

            const string currentTotalCmdFolder = TempFolder + "\\TotalCmd";
            const string currentTotalCmdPath = currentTotalCmdFolder + "\\TOTALCMD.EXE";
            _registryService.TotalCommanderFolder = currentTotalCmdFolder;

            DirectoryUtility.EnsureClearFolder(currentTotalCmdFolder);
            File.WriteAllText(currentTotalCmdPath, string.Empty);

            Settings actualSettings = _settingsSerializer.Deserialize();

            Assert.That(actualSettings.TotalCommanderPath, Is.EqualTo(currentTotalCmdPath));
        }

        [Test]
        public void Deserialize_TotalCommanderDoesntExistRegistryPathDoesntExists_ReturnNull()
        {
            File.WriteAllText(TotalCommanderPath, string.Empty);
            Settings expectedSettings = new Settings
            {
                TotalCommanderPath = TotalCommanderPath
            };
            _settingsSerializer.Serialize(expectedSettings);

            File.Delete(TotalCommanderPath);

            const string currentTotalCmdFolder = TempFolder + "\\TotalCmd";
            const string currentTotalCmdPath = currentTotalCmdFolder + "\\TotalCmd\\TOTALCMD.EXE";
            _registryService.TotalCommanderFolder = currentTotalCmdPath;

            Settings actualSettings = _settingsSerializer.Deserialize();

            Assert.That(actualSettings.TotalCommanderPath, Is.Null);
        }

        [Test]
        public void Deserialize_AfterRunOnStartUpChangedInRegistry_ReturnUpdatedValue()
        {
            Settings expectedSettings = new Settings
            {
                RunOnStartup = true
            };
            _settingsSerializer.Serialize(expectedSettings);

            _registryService.RunOnStartUp = false;
            Settings actualSettings = _settingsSerializer.Deserialize();

            Assert.That(actualSettings.RunOnStartup, Is.False);
        }

        [Test]
        public void Deserialize_InvalidFile_ReturnDefaultSettings()
        {
            DirectoryUtility.EnsureClearFolder(Path.GetDirectoryName(SettingsFilePath));
            File.WriteAllText(SettingsFilePath, "asdasdasd");
            Settings actualSettings = _settingsSerializer.Deserialize();

            Assert.That(actualSettings.GlobalKeyCombination, Is.EqualTo(Keys.Control | Keys.Shift | Keys.M));
        }
    }
}
