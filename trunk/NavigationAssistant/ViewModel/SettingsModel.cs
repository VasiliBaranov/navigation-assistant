using System.Collections.Generic;
using System.Windows.Forms;
using NavigationAssistant.PresentationModel;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.PresentationServices.Implementations;

namespace NavigationAssistant.ViewModel
{
    public class SettingsModel
    {
        public SettingsModel()
        {
            ISettingsSerializer settingsSerializer = new SettingsSerializer();
            Settings settings = settingsSerializer.Deserialize();

            if (settings == null)
            {
                settings = new Settings();
            }

            settings.FoldersToParse = new List<string> { "E:\\", "D:\\" };
            settings.IncludeHiddenFolders = true;
            settings.PrimaryNavigator = Navigators.WindowsExplorer;
            settings.AdditionalNavigators = new List<Navigators> { Navigators.TotalCommander };
            settings.CacheUpdateIntervalInSeconds = 200;
            settings.CacheFolder = Application.LocalUserAppDataPath;

            //settingsSerializer.Serialize(settings);

            MessageBox.Show("Settings screen!");
        }
    }
}
