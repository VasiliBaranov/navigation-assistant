using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using Core.Utilities;
using NavigationAssistant.PresentationModel;

namespace NavigationAssistant.PresentationServices.Implementations
{
    //With *.settings files approach app.config become awful.
    //Custom Configuration Sections approach is weird and too verbose.
    //So we use an approach from http://msdn.microsoft.com/en-us/library/ms973902.aspx
    //with custom xml serialization of settings to user folders.
    public class SettingsSerializer : ISettingsSerializer
    {
        private const string SettingsFileName = "UserSettings.config";

        public Settings Deserialize()
        {
            string settingsFileName = GetSettingsFileName(false);
            if (!File.Exists(settingsFileName))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            Settings settings;
            using (StreamReader reader = new StreamReader(settingsFileName))
            {
                settings = serializer.Deserialize(reader) as Settings;
            }

            return settings;
        }

        public void Serialize(Settings settings)
        {
            string settingsFileName = GetSettingsFileName(true);

            XmlSerializer serializer = new XmlSerializer(typeof (Settings));

            using (StreamWriter writer = new StreamWriter(settingsFileName, false))
            {
                serializer.Serialize(writer, settings);
            }
        }

        private string GetSettingsFileName(bool ensureFolder)
        {
            string settingsFolder = Application.LocalUserAppDataPath;
            if (ensureFolder)
            {
                Utility.EnsureFolder(settingsFolder);
            }

            return Path.Combine(settingsFolder, SettingsFileName);
        }
    }
}
